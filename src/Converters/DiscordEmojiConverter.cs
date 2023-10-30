using System.Threading.Tasks;
using DSharpPlus.CommandAll.Processors.SlashCommands;
using DSharpPlus.CommandAll.Processors.TextCommands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace DSharpPlus.CommandAll.Converters
{
    public class DiscordEmojiConverter : ISlashArgumentConverter<DiscordEmoji>, ITextArgumentConverter<DiscordEmoji>
    {
        public ApplicationCommandOptionType ArgumentType { get; init; } = ApplicationCommandOptionType.String;

        public Task<Optional<DiscordEmoji>> ConvertAsync(ConverterContext context, MessageCreateEventArgs eventArgs) => throw new System.NotImplementedException();
        public Task<Optional<DiscordEmoji>> ConvertAsync(ConverterContext context, InteractionCreateEventArgs eventArgs)
        {
            string? emojiString = context.As<SlashConverterContext>().CurrentOption.Value.ToString();
            return !string.IsNullOrWhiteSpace(emojiString)
                ? ConvertAsync(context, emojiString)
                : Task.FromResult(Optional.FromNoValue<DiscordEmoji>());
        }

        public static Task<Optional<DiscordEmoji>> ConvertAsync(ConverterContext context, string value) => DiscordEmoji.TryFromUnicode(context.Client, value, out DiscordEmoji? emoji) || DiscordEmoji.TryFromName(context.Client, value, out emoji)
            ? Task.FromResult(Optional.FromValue(emoji))
            : Task.FromResult(Optional.FromNoValue<DiscordEmoji>());
    }
}
