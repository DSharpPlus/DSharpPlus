namespace DSharpPlus.CommandAll.Processors.TextCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandAll.Commands;
using DSharpPlus.CommandAll.Converters;
using DSharpPlus.CommandAll.EventArgs;
using DSharpPlus.CommandAll.Exceptions;
using DSharpPlus.CommandAll.Processors.TextCommands.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.DependencyInjection;

public sealed class TextCommandProcessor(TextCommandConfiguration? configuration = null) : BaseCommandProcessor<MessageCreateEventArgs, ITextArgumentConverter, TextConverterContext, TextCommandContext>
{
    public TextCommandConfiguration Configuration { get; init; } = configuration ?? new();
    private bool _configured;

    public override async Task ConfigureAsync(CommandAllExtension extension)
    {
        await base.ConfigureAsync(extension);
        if (!this._configured)
        {
            this._configured = true;
            if (!extension.Client.Intents.HasIntent(DiscordIntents.MessageContents))
            {
                TextLogging.MissingMessageContentIntent(this._logger, null);
                return;
            }

            extension.Client.MessageCreated += this.ExecuteTextCommandAsync;
        }
    }

    public async Task ExecuteTextCommandAsync(DiscordClient client, MessageCreateEventArgs eventArgs)
    {
        if (this._extension is null)
        {
            throw new InvalidOperationException("TextCommandProcessor has not been configured.");
        }
        else if (eventArgs.Message.Content.Length == 0
            || (eventArgs.Author.IsBot && this.Configuration.IgnoreBots)
            || (this._extension.DebugGuildId.HasValue && eventArgs.Guild?.Id != this._extension.DebugGuildId))
        {
            return;
        }

        int prefixLength = await this.Configuration.PrefixResolver(this._extension, eventArgs.Message);
        if (prefixLength < 0)
        {
            return;
        }

        string commandText = eventArgs.Message.Content[prefixLength..];
        int index = commandText.IndexOf(' ');
        if (index == -1)
        {
            index = commandText.Length;
        }

        AsyncServiceScope scope = this._extension.ServiceProvider.CreateAsyncScope();
        if (!this._extension.Commands.TryGetValue(commandText[..index], out Command? command))
        {
            // Search for any aliases
            foreach (Command officialCommand in this._extension.Commands.Values)
            {
                TextAliasAttribute? aliasAttribute = officialCommand.Attributes.OfType<TextAliasAttribute>().FirstOrDefault();
                if (aliasAttribute is not null && aliasAttribute.Aliases.Any(alias => alias.Equals(commandText[..index], StringComparison.OrdinalIgnoreCase)))
                {
                    command = officialCommand;
                    break;
                }
            }

            // No alias was found
            if (command is null)
            {
                await this._extension._commandErrored.InvokeAsync(this._extension, new CommandErroredEventArgs()
                {
                    Context = new TextCommandContext()
                    {
                        Arguments = new Dictionary<CommandParameter, object?>(),
                        Channel = eventArgs.Channel,
                        Command = null!,
                        Extension = this._extension,
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
                break;
            }

            // Backspace once to trim the space
            nextIndex--;

            // Resolve subcommands
            Command? foundCommand = command.Subcommands.FirstOrDefault(command => command.Name.Equals(commandText[index..nextIndex], StringComparison.OrdinalIgnoreCase));
            if (foundCommand is null)
            {
                break;
            }

            foundCommand = command.Subcommands.FirstOrDefault(command => command.Attributes.OfType<TextAliasAttribute>().FirstOrDefault()?.Aliases.Any(alias => alias.Equals(commandText[index..nextIndex], StringComparison.OrdinalIgnoreCase)) ?? false);
            if (foundCommand is null)
            {
                break;
            }

            index = nextIndex;
            command = foundCommand;
        }

        TextConverterContext converterContext = new()
        {
            Channel = eventArgs.Channel,
            Command = command,
            Extension = this._extension,
            RawArguments = commandText[index..],
            ServiceScope = scope,
            Splicer = this.Configuration.TextArgumentSplicer,
            User = eventArgs.Author
        };

        TextCommandContext? commandContext = await this.ParseArgumentsAsync(converterContext, eventArgs);
        if (commandContext is null)
        {
            await scope.DisposeAsync();
            return;
        }

        await this._extension.CommandExecutor.ExecuteAsync(commandContext);
    }

    public override TextCommandContext CreateCommandContext(TextConverterContext converterContext, MessageCreateEventArgs eventArgs, Dictionary<CommandParameter, object?> parsedArguments) => new()
    {
        Arguments = parsedArguments,
        Channel = eventArgs.Channel,
        Command = converterContext.Command,
        Extension = this._extension ?? throw new InvalidOperationException("TextCommandProcessor has not been configured."),
        Message = eventArgs.Message,
        ServiceScope = converterContext.ServiceScope,
        User = eventArgs.Author
    };

    protected override async Task<IOptional> ExecuteConvertAsync<T>(ITextArgumentConverter converter, TextConverterContext converterContext, MessageCreateEventArgs eventArgs)
    {
        IArgumentConverter<MessageCreateEventArgs, T> strongConverter = (IArgumentConverter<MessageCreateEventArgs, T>)converter;
        if (converterContext.Parameter.Attributes.OfType<ParamArrayAttribute>().Any())
        {
            List<T> values = [];
            do
            {
                Optional<T> optional = await strongConverter.ConvertAsync(converterContext, eventArgs);
                if (!optional.HasValue)
                {
                    break;
                }

                values.Add(optional.Value);
            } while (converterContext.NextArgument());
            return Optional.FromValue(values.ToArray());
        }

        if (!converter.RequiresText || converterContext.NextArgument())
        {
            return await base.ExecuteConvertAsync<T>(converter, converterContext, eventArgs);
        }
        else if (converterContext.Parameter.DefaultValue.HasValue)
        {
            return Optional.FromValue(converterContext.Parameter.DefaultValue.Value);
        }

        throw new ArgumentParseException(converterContext.Parameter, message: $"Missing text argument for {converterContext.Parameter.Name}.");
    }
}
