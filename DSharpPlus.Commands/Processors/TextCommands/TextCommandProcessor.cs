using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DSharpPlus.Commands.EventArgs;
using DSharpPlus.Commands.Exceptions;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Commands.Trees.Metadata;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

using Microsoft.Extensions.DependencyInjection;

namespace DSharpPlus.Commands.Processors.TextCommands;

public sealed class TextCommandProcessor(TextCommandConfiguration? configuration = null) : BaseCommandProcessor<MessageCreatedEventArgs, ITextArgumentConverter, TextConverterContext, TextCommandContext>
{
    public const DiscordIntents RequiredIntents = DiscordIntents.DirectMessages // Required for commands executed in DMs
                                                | DiscordIntents.GuildMessages; // Required for commands that are executed via bot ping

    public TextCommandConfiguration Configuration { get; init; } = configuration ?? new();
    private bool configured;

    private FrozenDictionary<string, Command> commands;
    public override IReadOnlyList<Command> Commands => this.commands.Values;

    /// <inheritdoc />
    public override async ValueTask ConfigureAsync(CommandsExtension extension)
    {
        await base.ConfigureAsync(extension);

        Dictionary<string, Command> textCommands = [];

        foreach (Command command in this.extension.GetCommandsForProcessor(this))
        {
            textCommands.Add(command.Name, command);
        }
 
        this.commands = textCommands.ToFrozenDictionary(this.Configuration.CommandNameComparer);
        
        if (this.configured)
        {
            return;
        }

        this.configured = true;
        extension.Client.MessageCreated += ExecuteTextCommandAsync;
        if (!extension.Client.Intents.HasIntent(DiscordIntents.GuildMessages) && !extension.Client.Intents.HasIntent(DiscordIntents.DirectMessages))
        {
            TextLogging.MissingRequiredIntents(this.logger, RequiredIntents, null);
        }
        else if (!extension.Client.Intents.HasIntent(DiscordIntents.MessageContents) && !this.Configuration.SuppressMissingMessageContentIntentWarning)
        {
            TextLogging.MissingMessageContentIntent(this.logger, null);
        }
    }

    public async Task ExecuteTextCommandAsync(DiscordClient client, MessageCreatedEventArgs eventArgs)
    {
        if (this.extension is null)
        {
            throw new InvalidOperationException("TextCommandProcessor has not been configured.");
        }
        else if (string.IsNullOrWhiteSpace(eventArgs.Message.Content)
            || (eventArgs.Author.IsBot && this.Configuration.IgnoreBots)
            || (this.extension.DebugGuildId != 0 && this.extension.DebugGuildId != eventArgs.Guild?.Id))
        {
            return;
        }

        int prefixLength = await this.Configuration.PrefixResolver(this.extension, eventArgs.Message);
        if (prefixLength < 0)
        {
            return;
        }

        // Remove the prefix
        string commandText = eventArgs.Message.Content[prefixLength..].TrimStart();

        // Declare the index here for scope, keep reading until a whitespace character is found.
        int index;
        for (index = 0; index < commandText.Length; index++)
        {
            if (char.IsWhiteSpace(commandText[index]))
            {
                break;
            }
        }

        AsyncServiceScope scope = this.extension.ServiceProvider.CreateAsyncScope();
        if (!this.commands.TryGetValue(commandText[..index], out Command? command))
        {
            // Search for any aliases
            foreach (Command officialCommand in this.commands.Values)
            {
                TextAliasAttribute? aliasAttribute = officialCommand.Attributes.OfType<TextAliasAttribute>().FirstOrDefault();
                if (aliasAttribute is not null && aliasAttribute.Aliases.Any(alias => alias.Equals(commandText[..index], StringComparison.OrdinalIgnoreCase)))
                {
                    command = officialCommand;
                    break;
                }
            }
        }

        // No alias was found
        if (command is null ||
            (command.GuildIds.Count > 0 && !command.GuildIds.Contains(eventArgs.Guild?.Id ?? 0)))
        {
            await this.extension.commandErrored.InvokeAsync(this.extension, new CommandErroredEventArgs()
            {
                Context = new TextCommandContext()
                {
                    Arguments = new Dictionary<CommandParameter, object?>(),
                    Channel = eventArgs.Channel,
                    Command = null!,
                    Extension = this.extension,
                    Message = eventArgs.Message,
                    ServiceScope = scope,
                    User = eventArgs.Author
                },
                Exception = new CommandNotFoundException(commandText[..index]),
                CommandObject = null
            });

            await scope.DisposeAsync();
            return;
        }

        // If there is a space after the command's name, skip it.
        if (index < commandText.Length && commandText[index] == ' ')
        {
            index++;
        }

        // Recursively resolve subcommands
        int nextIndex = index;
        while (nextIndex != -1)
        {
            // If the index is at the end of the string, break
            if (nextIndex >= commandText.Length)
            {
                break;
            }

            // If there was no space found after the subcommand, break
            nextIndex = commandText.IndexOf(' ', nextIndex);
            if (nextIndex == -1)
            {
                // No more spaces. Search the rest of the string to see if there is a subcommand that matches.
                nextIndex = commandText.Length;
            }

            // Resolve subcommands
            Command? foundCommand = command.Subcommands.FirstOrDefault(command => command.Name.Equals(commandText[index..nextIndex], StringComparison.OrdinalIgnoreCase));
            if (foundCommand is null)
            {
                foundCommand = command.Subcommands.FirstOrDefault(command => command.Attributes.OfType<TextAliasAttribute>().FirstOrDefault()?.Aliases.Any(alias => alias.Equals(commandText[index..nextIndex], StringComparison.OrdinalIgnoreCase)) ?? false);
                if (foundCommand is null)
                {
                    break;
                }
            }

            index = nextIndex;
            command = foundCommand;
        }

        if (command.Method is null)
        {
            Command? defaultGroupCommand = command.Subcommands.FirstOrDefault(subcommand => subcommand.Attributes.OfType<DefaultGroupCommandAttribute>().Any());
            if (defaultGroupCommand is null)
            {
                await this.extension.commandErrored.InvokeAsync(this.extension, new CommandErroredEventArgs()
                {
                    Context = CreateCommandContext(new()
                    {
                        Channel = eventArgs.Channel,
                        Command = command,
                        Extension = this.extension,
                        RawArguments = commandText[index..],
                        ServiceScope = scope,
                        Splicer = this.Configuration.TextArgumentSplicer,
                        User = eventArgs.Author
                    }, eventArgs, []),
                    Exception = new CommandNotExecutableException(command, "Unable to execute a command that has no method. Is this command a group command?"),
                    CommandObject = null
                });

                return;
            }

            command = defaultGroupCommand;
        }

        TextConverterContext converterContext = new()
        {
            Channel = eventArgs.Channel,
            Command = command,
            Extension = this.extension,
            RawArguments = commandText[index..],
            ServiceScope = scope,
            Splicer = this.Configuration.TextArgumentSplicer,
            User = eventArgs.Author
        };

        TextCommandContext? commandContext = await ParseArgumentsAsync(converterContext, eventArgs);
        if (commandContext is null)
        {
            await scope.DisposeAsync();
            return;
        }

        await this.extension.CommandExecutor.ExecuteAsync(commandContext);
    }

    public override TextCommandContext CreateCommandContext
    (
        TextConverterContext converterContext,
        MessageCreatedEventArgs eventArgs,
        Dictionary<CommandParameter, object?> parsedArguments
    )
    {
        return new()
        {
            Arguments = parsedArguments,
            Channel = eventArgs.Channel,
            Command = converterContext.Command,
            Extension = this.extension ?? throw new InvalidOperationException("TextCommandProcessor has not been configured."),
            Message = eventArgs.Message,
            ServiceScope = converterContext.ServiceScope,
            User = eventArgs.Author
        };
    }

    /// <inheritdoc/>
    protected override async ValueTask<IOptional> ExecuteConverterAsync<T>
    (
        ITextArgumentConverter converter,
        TextConverterContext converterContext,
        MessageCreatedEventArgs eventArgs
    )
    {   
        if (converter is not ITextArgumentConverter<T> typedConverter)
        {
            throw new InvalidOperationException("The provided converter was of the wrong type.");
        }

        if (!converterContext.NextArgument())
        {
            return converterContext.Parameter.DefaultValue.HasValue
                ? Optional.FromValue(converterContext.Parameter.DefaultValue.Value)
                : throw new ArgumentParseException(converterContext.Parameter, message: $"Missing argument for {converterContext.Parameter.Name}.");
        }
        else if (!converterContext.Parameter.Attributes.OfType<ParamArrayAttribute>().Any())
        {
            return await typedConverter.ConvertAsync(converterContext, eventArgs);
        }

        List<T> values = [];
        
        do
        {
            Optional<T> optional = await typedConverter.ConvertAsync(converterContext, eventArgs);

            if (!optional.HasValue)
            {
                break;
            }

            values.Add(optional.Value);
        } while (converterContext.NextArgument());

        return Optional.FromValue(values.ToArray());
    }

    /// <inheritdoc/>
    public override async ValueTask<TextCommandContext?> ParseArgumentsAsync
    (
        TextConverterContext converterContext, 
        MessageCreatedEventArgs eventArgs
    )
    {
        if (this.extension is null)
        {
            return null;
        }

        Dictionary<CommandParameter, object?> parsedArguments = new(converterContext.Command.Parameters.Count);

        foreach (CommandParameter parameter in converterContext.Command.Parameters)
        {
            parsedArguments.Add(parameter, new ConverterSentinel());
        }

        try
        {
            while (converterContext.NextParameter())
            {
                IOptional optional = await this.ConverterDelegates[GetConverterFriendlyBaseType(converterContext.Parameter.Type)](converterContext, eventArgs);

                if (!optional.HasValue)
                {
                    await this.extension.commandErrored.InvokeAsync(converterContext.Extension, new CommandErroredEventArgs()
                    {
                        Context = CreateCommandContext(converterContext, eventArgs, parsedArguments),
                        Exception = new ArgumentParseException(converterContext.Parameter, null, $"Argument Converter for type {converterContext.Parameter.Type.FullName} was unable to parse the argument."),
                        CommandObject = null
                    });

                    return null;
                }

                parsedArguments[converterContext.Parameter] = optional.RawValue;
            }

            if (parsedArguments.Any(x => x.Value is ConverterSentinel))
            {
                // Try to fill with default values
                foreach (CommandParameter parameter in converterContext.Command.Parameters)
                {
                    if (parsedArguments[parameter] is not ConverterSentinel)
                    {
                        continue;
                    }

                    if (!parameter.DefaultValue.HasValue)
                    {
                        await this.extension.commandErrored.InvokeAsync(converterContext.Extension, new CommandErroredEventArgs()
                        {
                            Context = CreateCommandContext(converterContext, eventArgs, parsedArguments),
                            Exception = new ArgumentParseException(converterContext.Parameter, null, "No value was provided for this parameter."),
                            CommandObject = null
                        });

                        return null;
                    }

                    parsedArguments[parameter] = parameter.DefaultValue.Value;
                }
            }
        }
        catch (Exception error)
        {
            await this.extension.commandErrored.InvokeAsync(converterContext.Extension, new CommandErroredEventArgs()
            {
                Context = CreateCommandContext(converterContext, eventArgs, parsedArguments),
                Exception = new ArgumentParseException(converterContext.Parameter, error),
                CommandObject = null
            });

            return null;
        }

        return CreateCommandContext(converterContext, eventArgs, parsedArguments);
    }
}
