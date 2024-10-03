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

public sealed class TextCommandProcessor : BaseCommandProcessor<ITextArgumentConverter, TextConverterContext, TextCommandContext>
{
    public const DiscordIntents RequiredIntents = DiscordIntents.DirectMessages // Required for commands executed in DMs
                                                | DiscordIntents.GuildMessages; // Required for commands that are executed via bot ping

    public TextCommandConfiguration Configuration { get; init; }

    public override IReadOnlyList<Command> Commands => this.commands.Values;
    private FrozenDictionary<string, Command> commands = FrozenDictionary<string, Command>.Empty;

    /// <summary>
    /// Creates a new instance of <see cref="TextCommandProcessor"/> with the default configuration.
    /// </summary>
    public TextCommandProcessor() : this(new TextCommandConfiguration()) { }

    /// <summary>
    /// Creates a new instance of <see cref="TextCommandProcessor"/> with the specified configuration.
    /// </summary>
    /// <param name="configuration">The configuration to use with this processor.</param>
    public TextCommandProcessor(TextCommandConfiguration configuration) => this.Configuration = configuration;

    /// <inheritdoc />
    public override async ValueTask ConfigureAsync(CommandsExtension extension)
    {
        Dictionary<string, Command> textCommands = [];
        foreach (Command command in extension.GetCommandsForProcessor(this))
        {
            textCommands.Add(command.Name, command);
        }

        this.commands = textCommands.ToFrozenDictionary(this.Configuration.CommandNameComparer);
        if (this.extension is null)
        {
            extension.Client.MessageCreated += ExecuteTextCommandAsync;

            // Put these logs here so that they only appear when the processor is configured the first time.
            if (!extension.Client.Intents.HasIntent(DiscordIntents.GuildMessages) && !extension.Client.Intents.HasIntent(DiscordIntents.DirectMessages))
            {
                TextLogging.missingRequiredIntents(this.logger, RequiredIntents, null);
            }
            else if (!extension.Client.Intents.HasIntent(DiscordIntents.MessageContents) && !this.Configuration.SuppressMissingMessageContentIntentWarning)
            {
                TextLogging.missingMessageContentIntent(this.logger, null);
            }
        }

        await base.ConfigureAsync(extension);
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

        AsyncServiceScope serviceScope = this.extension.ServiceProvider.CreateAsyncScope();
        ResolvePrefixDelegateAsync resolvePrefix = serviceScope.ServiceProvider.GetService<IPrefixResolver>() is IPrefixResolver prefixResolver
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
            if (this.Configuration!.EnableCommandNotFoundException)
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
                        ServiceScope = serviceScope,
                        User = eventArgs.Author
                    },
                    Exception = new CommandNotFoundException(commandText[..index]),
                    CommandObject = null
                });
            }

            await serviceScope.DisposeAsync();
            return;
        }

        // If this is a group command, try to see if it's executable.
        if (command.Method is null)
        {
            Command? defaultGroupCommand = command.Subcommands.FirstOrDefault(subcommand => subcommand.Attributes.OfType<DefaultGroupCommandAttribute>().Any());
            if (defaultGroupCommand is null)
            {
                if (this.Configuration!.EnableCommandNotFoundException)
                {
                    await this.extension.commandErrored.InvokeAsync(this.extension, new CommandErroredEventArgs()
                    {
                        Context = new TextCommandContext()
                        {
                            Arguments = new Dictionary<CommandParameter, object?>(),
                            Channel = eventArgs.Channel,
                            Command = command,
                            Extension = this.extension,
                            Message = eventArgs.Message,
                            ServiceScope = serviceScope,
                            User = eventArgs.Author
                        },
                        Exception = new CommandNotExecutableException(command, "Unable to execute a command that has no method. Is this command a group command?"),
                        CommandObject = null
                    });
                }

                await serviceScope.DisposeAsync();
                return;
            }

            command = defaultGroupCommand;
        }

        TextConverterContext converterContext = new()
        {
            Channel = eventArgs.Channel,
            Command = command,
            Extension = this.extension,
            Message = eventArgs.Message,
            RawArguments = commandText[index..],
            ServiceScope = serviceScope,
            Splicer = this.Configuration.TextArgumentSplicer,
            User = eventArgs.Author
        };

        IReadOnlyDictionary<CommandParameter, object?> parsedArguments = await ParseParametersAsync(converterContext);
        TextCommandContext commandContext = CreateCommandContext(converterContext, parsedArguments);

        // Iterate over all arguments and check if any of them failed to parse.
        foreach (KeyValuePair<CommandParameter, object?> argument in parsedArguments)
        {
            if (argument.Value is ArgumentFailedConversionValue argumentFailedConversionValue)
            {
                await this.extension.commandErrored.InvokeAsync(this.extension, new CommandErroredEventArgs()
                {
                    Context = commandContext,
                    CommandObject = null,
                    Exception = new ArgumentParseException(argument.Key, argumentFailedConversionValue.Error)
                });

                await serviceScope.DisposeAsync();
                return;
            }
            else if (argument.Value is ArgumentNotParsedValue)
            {
                await this.extension.commandErrored.InvokeAsync(this.extension, new CommandErroredEventArgs()
                {
                    Context = commandContext,
                    CommandObject = null,
                    Exception = new ArgumentParseException(argument.Key, new ArgumentException("Argument could not be parsed."))
                });

                await serviceScope.DisposeAsync();
                return;
            }
        }

        await this.extension.CommandExecutor.ExecuteAsync(commandContext);
    }

    public override TextCommandContext CreateCommandContext(TextConverterContext converterContext, IReadOnlyDictionary<CommandParameter, object?> parsedArguments)
    {
        return new()
        {
            Arguments = parsedArguments,
            Channel = converterContext.Channel,
            Command = converterContext.Command,
            Extension = this.extension ?? throw new InvalidOperationException("TextCommandProcessor has not been configured."),
            Message = converterContext.Message,
            ServiceScope = converterContext.ServiceScope,
            User = converterContext.User
        };
    }

    /// <inheritdoc/>
    protected override async ValueTask<IOptional> ExecuteConverterAsync<T>(ITextArgumentConverter converter, TextConverterContext converterContext)
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
            return await typedConverter.ConvertAsync(converterContext);
        }

        List<T> values = [];

        do
        {
            Optional<T> optional = await typedConverter.ConvertAsync(converterContext);
            if (!optional.HasValue)
            {
                break;
            }

            values.Add(optional.Value);
        } while (converterContext.NextArgument());

        return Optional.FromValue(values.ToArray());
    }

    /// <summary>
    /// Attempts to retrieve a command from the provided command text. Searches for the command by name, then by alias. Subcommands are also resolved.
    /// This method ignores <see cref="DefaultGroupCommandAttribute"/>'s and will instead return the group command instead of the default subcommand.
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
                foreach (Command subcommand in command.Subcommands)
                {
                    foreach (Attribute attribute in subcommand.Attributes)
                    {
                        if (attribute is not TextAliasAttribute aliasAttribute)
                        {
                            continue;
                        }

                        foreach (string alias in aliasAttribute.Aliases)
                        {
                            if (this.Configuration.CommandNameComparer.Equals(alias, subcommandName))
                            {
                                foundCommand = subcommand;
                                break;
                            }
                        }
                    }
                }

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
