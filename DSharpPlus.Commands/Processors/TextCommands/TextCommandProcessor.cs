namespace DSharpPlus.Commands.Processors.TextCommands;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Commands.EventArgs;
using DSharpPlus.Commands.Exceptions;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Commands.Trees.Metadata;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.DependencyInjection;

public sealed class TextCommandProcessor(TextCommandConfiguration? configuration = null) : BaseCommandProcessor<MessageCreateEventArgs, ITextArgumentConverter, TextConverterContext, TextCommandContext>
{
    public const DiscordIntents RequiredIntents = DiscordIntents.DirectMessages // Required for commands executed in DMs
                                                | DiscordIntents.GuildMessages; // Required for commands that are executed via bot ping

    public TextCommandConfiguration Configuration { get; init; } = configuration ?? new();
    private bool _configured;

    public override async ValueTask ConfigureAsync(CommandsExtension extension)
    {
        await base.ConfigureAsync(extension);
        if (_configured)
        {
            return;
        }

        _configured = true;
        extension.Client.MessageCreated += ExecuteTextCommandAsync;
        if (!extension.Client.Intents.HasIntent(DiscordIntents.GuildMessages) && !extension.Client.Intents.HasIntent(DiscordIntents.DirectMessages))
        {
            TextLogging.MissingRequiredIntents(_logger, RequiredIntents, null);
        }
        else if (!extension.Client.Intents.HasIntent(DiscordIntents.MessageContents) && !Configuration.SuppressMissingMessageContentIntentWarning)
        {
            TextLogging.MissingMessageContentIntent(_logger, null);
        }
    }

    public async Task ExecuteTextCommandAsync(DiscordClient client, MessageCreateEventArgs eventArgs)
    {
        if (_extension is null)
        {
            throw new InvalidOperationException("TextCommandProcessor has not been configured.");
        }
        else if (string.IsNullOrWhiteSpace(eventArgs.Message.Content)
            || (eventArgs.Author.IsBot && Configuration.IgnoreBots)
            || (_extension.DebugGuildId != 0 && _extension.DebugGuildId != eventArgs.Guild?.Id))
        {
            return;
        }

        int prefixLength = await Configuration.PrefixResolver(_extension, eventArgs.Message);
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

        AsyncServiceScope scope = _extension.ServiceProvider.CreateAsyncScope();
        if (!_extension.Commands.TryGetValue(commandText[..index], out Command? command))
        {
            // Search for any aliases
            foreach (Command officialCommand in _extension.Commands.Values)
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
            await _extension._commandErrored.InvokeAsync(_extension, new CommandErroredEventArgs()
            {
                Context = new TextCommandContext()
                {
                    Arguments = new Dictionary<CommandParameter, object?>(),
                    Channel = eventArgs.Channel,
                    Command = null!,
                    Extension = _extension,
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
                await _extension._commandErrored.InvokeAsync(_extension, new CommandErroredEventArgs()
                {
                    Context = CreateCommandContext(new()
                    {
                        Channel = eventArgs.Channel,
                        Command = command,
                        Extension = _extension,
                        RawArguments = commandText[index..],
                        ServiceScope = scope,
                        Splicer = Configuration.TextArgumentSplicer,
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
            Extension = _extension,
            RawArguments = commandText[index..],
            ServiceScope = scope,
            Splicer = Configuration.TextArgumentSplicer,
            User = eventArgs.Author
        };

        TextCommandContext? commandContext = await ParseArgumentsAsync(converterContext, eventArgs);
        if (commandContext is null)
        {
            await scope.DisposeAsync();
            return;
        }

        await _extension.CommandExecutor.ExecuteAsync(commandContext);
    }

    public override TextCommandContext CreateCommandContext(TextConverterContext converterContext, MessageCreateEventArgs eventArgs, Dictionary<CommandParameter, object?> parsedArguments) => new()
    {
        Arguments = parsedArguments,
        Channel = eventArgs.Channel,
        Command = converterContext.Command,
        Extension = _extension ?? throw new InvalidOperationException("TextCommandProcessor has not been configured."),
        Message = eventArgs.Message,
        ServiceScope = converterContext.ServiceScope,
        User = eventArgs.Author
    };
}
