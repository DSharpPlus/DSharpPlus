using DSharpPlus.Entities;

namespace DSharpPlus.CH.Message;

public interface IFailedErrors
{
    public Task HandleConversionAsync(InvalidMessageConvertionError error, DiscordMessage message);

    public Task HandleUnhandledExceptionAsync(Exception e, DiscordMessage message);
}
