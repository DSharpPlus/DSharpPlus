using DSharpPlus.Entities;
using Remora.Results;

namespace DSharpPlus.UnifiedCommands.Message.Converters;

public sealed class StringConverter : IMessageConverter<string?>
{
    public IResult ConvertValue(DiscordClient client, DiscordMessage message,
        ArraySegment<char> segment)
            => Result<string?>.FromSuccess(new(segment));
}