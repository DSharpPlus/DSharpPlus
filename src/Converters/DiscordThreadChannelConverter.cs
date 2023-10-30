using System.Threading.Tasks;
using DSharpPlus.CommandAll.Processors.SlashCommands;
using DSharpPlus.CommandAll.Processors.TextCommands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace DSharpPlus.CommandAll.Converters
{
    public class DiscordThreadChannelConverter : ISlashArgumentConverter<DiscordThreadChannel>, ITextArgumentConverter<DiscordThreadChannel>
    {
        public ApplicationCommandOptionType ArgumentType { get; init; } = ApplicationCommandOptionType.Channel;

        public Task<Optional<DiscordThreadChannel>> ConvertAsync(ConverterContext context, MessageCreateEventArgs eventArgs) => throw new System.NotImplementedException();
        public Task<Optional<DiscordThreadChannel>> ConvertAsync(ConverterContext context, InteractionCreateEventArgs eventArgs)
        {
            SlashConverterContext slashContext = context.As<SlashConverterContext>();
            return Task.FromResult(Optional.FromValue((DiscordThreadChannel)slashContext.Interaction.Data.Resolved.Channels[(ulong)slashContext.CurrentOption.Value]));
        }
    }
}
