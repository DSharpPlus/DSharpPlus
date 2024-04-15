namespace DSharpPlus.Commands.Converters;

using System.Threading.Tasks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

public class DiscordEmojiConverter : ISlashArgumentConverter<DiscordEmoji>, ITextArgumentConverter<DiscordEmoji>
{
    public DiscordApplicationCommandOptionType ParameterType { get; init; } = DiscordApplicationCommandOptionType.String;
    public bool RequiresText { get; init; } = true;

    public Task<Optional<DiscordEmoji>> ConvertAsync(TextConverterContext context, MessageCreateEventArgs eventArgs) => ConvertAsync(context, context.Argument);
    public Task<Optional<DiscordEmoji>> ConvertAsync(InteractionConverterContext context, InteractionCreateEventArgs eventArgs) => ConvertAsync(context, context.Argument.RawValue);
    public static Task<Optional<DiscordEmoji>> ConvertAsync(ConverterContext context, string? value) => !string.IsNullOrWhiteSpace(value)
        && (DiscordEmoji.TryFromUnicode(context.Client, value, out DiscordEmoji? emoji) || DiscordEmoji.TryFromName(context.Client, value, out emoji))
            ? Task.FromResult(Optional.FromValue(emoji))
            : Task.FromResult(Optional.FromNoValue<DiscordEmoji>());
}
