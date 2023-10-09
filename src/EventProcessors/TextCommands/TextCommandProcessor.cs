using System.Threading.Tasks;
using DSharpPlus.CommandAll.Commands;
using DSharpPlus.CommandAll.Converters.Meta;
using DSharpPlus.CommandAll.EventArgs;
using DSharpPlus.EventArgs;

namespace DSharpPlus.CommandAll.EventProcessors
{
    public sealed class TextCommandProcessor : IEventProcessor<MessageCreateEventArgs>
    {
        public required TextCommandOptions Options { get; init; }
        private bool _eventsRegistered;

        public Task ConfigureAsync(CommandAllExtension extension, ConfigureCommandsEventArgs eventArgs)
        {
            if (_eventsRegistered)
            {
                return Task.CompletedTask;
            }

            extension.Client.MessageCreated += async (client, eventArgs) =>
            {
                // Test if we're debugging in a specific guild
                // If we ignore bots
                // If we ignore guilds
                // If we ignore DMs
                if ((extension.DebugGuildId is not null && eventArgs.Guild?.Id != extension.DebugGuildId)
                    || (eventArgs.Author.IsBot && !Options.HasFlag(TextCommandOptions.AllowBots))
                    || (eventArgs.Guild is null && !Options.HasFlag(TextCommandOptions.AllowDMs))
                    || (eventArgs.Guild is not null && !Options.HasFlag(TextCommandOptions.AllowGuilds)))
                {
                    return;
                }

                ConverterContext? converterContext = await ConvertEventAsync(extension, eventArgs);
                if (converterContext is null)
                {
                    return;
                }

                CommandContext? commandContext = await ParseArgumentsAsync(converterContext, eventArgs);
                if (commandContext is null)
                {
                    return;
                }

                await extension.CommandExecutor.ExecuteAsync(commandContext);
            };

            _eventsRegistered = true;
            return Task.CompletedTask;
        }

        public Task<ConverterContext?> ConvertEventAsync(CommandAllExtension extension, MessageCreateEventArgs eventArgs) => Task.FromResult<ConverterContext?>(null);
        public Task<CommandContext?> ParseArgumentsAsync(ConverterContext converterContext, MessageCreateEventArgs eventArgs) => Task.FromResult<CommandContext?>(null);
    }
}
