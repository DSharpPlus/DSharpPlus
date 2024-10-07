using System.Threading.Tasks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Entities;

namespace DSharpPlus.Commands.Converters;

public class DiscordEmojiConverter : ISlashArgumentConverter<DiscordEmoji>, ITextArgumentConverter<DiscordEmoji>
{
    public DiscordApplicationCommandOptionType ParameterType => DiscordApplicationCommandOptionType.String;
    public ConverterRequiresText RequiresText => ConverterRequiresText.Always;
    public string ReadableName => "Discord Emoji";

    public Task<Optional<DiscordEmoji>> ConvertAsync(ConverterContext context)
    {
        string? value = context.Argument?.ToString();
        return !string.IsNullOrWhiteSpace(value)
            // Unicode emoji's get priority
            && (DiscordEmoji.TryFromUnicode(context.Client, value, out DiscordEmoji? emoji) || DiscordEmoji.TryFromName(context.Client, value, out emoji))
                ? Task.FromResult(Optional.FromValue(emoji))
                : Task.FromResult(Optional.FromNoValue<DiscordEmoji>());
    }
}
