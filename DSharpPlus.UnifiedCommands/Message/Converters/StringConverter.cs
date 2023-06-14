using DSharpPlus.Entities;
using Remora.Results;

namespace DSharpPlus.UnifiedCommands.Message.Converters;

public sealed class StringConverter : IMessageConverter<string?>
{
    public IResult ConvertValue(DiscordClient client, DiscordMessage message,
        ArraySegment<char>? segment)
    {
        if (segment is ArraySegment<char> s)
        {
            Console.WriteLine($"Segment is {new string(s)}");
            return Result<string?>.FromSuccess(new(s));
        }
        else
        {
            Console.WriteLine("Segment is null");
            return Result<string?>.FromSuccess(null);
        }
    }
}