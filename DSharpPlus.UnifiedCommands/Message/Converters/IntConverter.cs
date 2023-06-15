using DSharpPlus.Entities;
using Remora.Results;

namespace DSharpPlus.UnifiedCommands.Message.Converters;

public class IntConverter : IMessageConverter<int>
{
    public IResult ConvertValue(DiscordClient client, DiscordMessage message, ArraySegment<char> segment)
    {
        return int.TryParse(segment, out int value)
                        ? Result<int>.FromSuccess(value)
                        : Result.FromError(new ExceptionError(new Exception("Couldn't parse string into a integer")));
    }

}