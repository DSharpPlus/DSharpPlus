using System.Threading.Tasks;
using DSharpPlus.CommandAll.Processors.SlashCommands;
using DSharpPlus.CommandAll.Processors.TextCommands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace DSharpPlus.CommandAll.Converters
{
    public class DiscordRoleConverter : ISlashArgumentConverter<DiscordRole>, ITextArgumentConverter<DiscordRole>
    {
        public ApplicationCommandOptionType ArgumentType { get; init; } = ApplicationCommandOptionType.Role;

        public Task<Optional<DiscordRole>> ConvertAsync(ConverterContext context, MessageCreateEventArgs eventArgs) => throw new System.NotImplementedException();
        public Task<Optional<DiscordRole>> ConvertAsync(ConverterContext context, InteractionCreateEventArgs eventArgs)
        {
            SlashConverterContext slashContext = context.As<SlashConverterContext>();
            return Task.FromResult(Optional.FromValue(slashContext.Interaction.Data.Resolved.Roles[(ulong)slashContext.CurrentOption.Value]));
        }
    }
}
