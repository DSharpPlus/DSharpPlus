using DSharpPlus.Entities;
using Remora.Results;

namespace DSharpPlus.UnifiedCommands.Message;

public interface IMessageConverter<T>
{
    public IResult ConvertValue(DiscordClient client, DiscordMessage message,
        ArraySegment<char>? segment);

    public ValueTask<IResult> ConvertValueAsync(DiscordClient client, DiscordMessage message,
        ArraySegment<char>? segment)
        => ValueTask.FromResult(ConvertValue(client, message, segment));
}
