using DSharpPlus.Entities;
using Remora.Results;

namespace DSharpPlus.UnifiedCommands.Message.Converters;

public class DiscordUserConverter : IMessageConverter<DiscordUser?>
{
    public IResult ConvertValue(DiscordClient client, DiscordMessage message, ArraySegment<char> segment) => throw new Exception();

    public async ValueTask<IResult> ConvertValueAsync(DiscordClient client, DiscordMessage message, ArraySegment<char> segment)
    {
        try
        {
            if (segment is ['<', '@', .., '>'])
            {
                ArraySegment<char> userIdSeg = segment[2] == '!' ? segment[3..^1] : segment[2..^1];

                return ulong.TryParse(userIdSeg, out ulong userId)
                    ? Result<DiscordUser>.FromSuccess(await client.GetUserAsync(userId))
                    : Result.FromError(new ExceptionError(new Exception("Invalid user")));

            }
            else
            {
                return ulong.TryParse(segment, out ulong userId)
                                            ? Result<DiscordUser>.FromSuccess(await client.GetUserAsync(userId))
                                            : Result.FromError(new ExceptionError(new Exception("Invalid user")));
            }
        }
        catch (DSharpPlus.Exceptions.NotFoundException e)
        {
            return Result.FromError(new ExceptionError(e));
        }
        catch (DSharpPlus.Exceptions.DiscordException e)
        {
            return Result.FromError(new ExceptionError(e));
        }
    }
}