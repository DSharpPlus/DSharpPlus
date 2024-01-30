namespace DSharpPlus.Commands.Converters;

using System.Threading.Tasks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

public class DiscordEmojiConverter : ISlashArgumentConverter<DiscordEmoji>, ITextArgumentConverter<DiscordEmoji>
{
    public ApplicationCommandOptionType ParameterType { get; init; } = ApplicationCommandOptionType.String;
    public bool RequiresText { get; init; } = true;

    public Task<Optional<DiscordEmoji>> ConvertAsync(ConverterContext context, MessageCreateEventArgs eventArgs) => ConvertAsync(context, context.As<TextConverterContext>().Argument);
    public Task<Optional<DiscordEmoji>> ConvertAsync(ConverterContext context, InteractionCreateEventArgs eventArgs) => ConvertAsync(context, context.As<SlashConverterContext>().Argument.Value.ToString());
    public static Task<Optional<DiscordEmoji>> ConvertAsync(ConverterContext context, string? value) => !string.IsNullOrWhiteSpace(value) && (DiscordEmoji.TryFromUnicode(context.Client, value, out DiscordEmoji? emoji) || DiscordEmoji.TryFromName(context.Client, value, out emoji))
        ? Task.FromResult(Optional.FromValue(emoji))
        : Task.FromResult(Optional.FromNoValue<DiscordEmoji>());
}
