using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

using DSharpPlus.Commands.EventArgs;
using DSharpPlus.Commands.Exceptions;
using DSharpPlus.Commands.Processors.TextCommands.Parsing;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Commands.Trees.Metadata;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

using Microsoft.Extensions.DependencyInjection;

namespace DSharpPlus.Commands.Processors.TextCommands;

/// <summary>
/// Handles the processing of text-based commands executed via Discord messages.
/// </summary>
public sealed class TextCommandProcessor(TextCommandConfiguration? configuration = null) : BaseCommandProcessor<MessageCreatedEventArgs, ITextArgumentConverter, TextConverterContext, TextCommandContext>
{
    /// <summary>
    /// The intents required for this processor to function.
    /// </summary>
    public const DiscordIntents RequiredIntents = DiscordIntents.DirectMessages // Required for commands executed in DMs
                                                | DiscordIntents.GuildMessages; // Required for commands that are executed via bot ping

    /// <summary>
    /// The configuration for this text command processor.
    /// </summary>
    public TextCommandConfiguration Configuration { get; init; } = configuration ?? new();
    private bool configured;

    /// <inheritdoc />
    public override IReadOnlyList<Command> Commands => this.commands.Values;
    private FrozenDictionary<string, Command> commands = new Dictionary<string, Command>().ToFrozenDictionary();

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

    /// <summary>
    /// Parses, resolves, and executes a text command.
    /// </summary>
    /// <param name="client">The client that received the message.</param>
    /// <param name="eventArgs">The event arguments containing the message.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
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

        AsyncServiceScope scope = this.extension.ServiceProvider.CreateAsyncScope();
        ResolvePrefixDelegateAsync resolvePrefix = scope.ServiceProvider.GetService<IPrefixResolver>() is IPrefixResolver prefixResolver
            ? prefixResolver.ResolvePrefixAsync
            : this.Configuration.PrefixResolver;

        int prefixLength = await resolvePrefix(this.extension, eventArgs.Message);
        if (prefixLength < 0)
        {
            return;
        }

        // Remove the prefix
        string commandText = eventArgs.Message.Content[prefixLength..].TrimStart();

        // Parse the full command name
        if (!TryGetCommand(commandText, eventArgs.Guild?.Id ?? 0, out int index, out Command? command))
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

        // If this is a group command, try to see if it's executable.
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

    /// <inheritdoc cref="TextExtensions.CreateFakeMessageEventArgs(CommandContext, DiscordMessage, string)" />
    public static MessageCreatedEventArgs CreateFakeMessageEventArgs(CommandContext context, DiscordMessage message, string content) => TextExtensions.CreateFakeMessageEventArgs(context, message, content);

    /// <inheritdoc/>
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

    /// <summary>
    /// Attempts to retrieve a command from the provided command text. Searches for the command by name, then by alias. Subcommands are also resolved. This method ignores <see cref="DefaultGroupCommandAttribute"/>'s and will instead return the group command instead of the default subcommand.
    /// </summary>
    /// <param name="commandText">The full command name and optionally it's arguments.</param>
    /// <param name="guildId">The guild ID to check if the command is available in the guild. Pass 0 if not applicable.</param>
    /// <param name="index">The index of <paramref name="commandText"/> that the command name ends at.</param>
    /// <param name="command">The resolved command.</param>
    /// <returns>If the command was found.</returns>
    public bool TryGetCommand(string commandText, ulong guildId, out int index, [NotNullWhen(true)] out Command? command)
    {
        // Declare the index here for scope, keep reading until a whitespace character is found.
        for (index = 0; index < commandText.Length; index++)
        {
            if (char.IsWhiteSpace(commandText[index]))
            {
                break;
            }
        }

        string rootCommandText = commandText[..index];
        if (!this.commands.TryGetValue(rootCommandText, out command))
        {
            // Search for any aliases
            foreach (Command officialCommand in this.commands.Values)
            {
                TextAliasAttribute? aliasAttribute = officialCommand.Attributes.OfType<TextAliasAttribute>().FirstOrDefault();
                if (aliasAttribute is not null && aliasAttribute.Aliases.Any(alias => this.Configuration.CommandNameComparer.Equals(alias, rootCommandText)))
                {
                    command = officialCommand;
                    return true;
                }
            }
        }

        // No alias was found
        if (command is null || (command.GuildIds.Count > 0 && !command.GuildIds.Contains(guildId)))
        {
            return false;
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
            string subcommandName = commandText[index..nextIndex];

            // Try searching for the subcommand by name, then by alias
            // We prioritize the name over the aliases to avoid a poor dev debugging experience
            Command? foundCommand = command.Subcommands.FirstOrDefault(subCommand => subCommand.Name.Equals(subcommandName, StringComparison.OrdinalIgnoreCase));
            if (foundCommand is null)
            {
                // Search for any aliases that the subcommand may have
                foundCommand = command.Subcommands.FirstOrDefault(subCommand => subCommand.Attributes.OfType<TextAliasAttribute>().FirstOrDefault()?.Aliases.Any(alias => this.Configuration.CommandNameComparer.Equals(alias, subcommandName)) ?? false);
                if (foundCommand is null)
                {
                    // There was no subcommand found by name or by alias.
                    // Maybe the index is on an argument for the command?
                    return true;
                }
            }

            // Try to parse the next subcommand
            index = nextIndex;
            command = foundCommand;
        }

        // We found it!! Good job!
        return true;
    }
}
