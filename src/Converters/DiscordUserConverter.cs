using System.Threading.Tasks;
using DSharpPlus.CommandAll.Processors.SlashCommands;
using DSharpPlus.CommandAll.Processors.TextCommands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace DSharpPlus.CommandAll.Converters
{
    public class DiscordMemberConverter : ISlashArgumentConverter<DiscordMember>, ITextArgumentConverter<DiscordMember>
    {
        public ApplicationCommandOptionType ArgumentType { get; init; } = ApplicationCommandOptionType.User;

        public Task<Optional<DiscordMember>> ConvertAsync(ConverterContext context, MessageCreateEventArgs eventArgs) => throw new System.NotImplementedException();
        public Task<Optional<DiscordMember>> ConvertAsync(ConverterContext context, InteractionCreateEventArgs eventArgs)
        {
            SlashConverterContext slashContext = context.As<SlashConverterContext>();
            return Task.FromResult(Optional.FromValue(slashContext.Interaction.Data.Resolved.Members[(ulong)slashContext.CurrentOption.Value]));
        }
    }
}
