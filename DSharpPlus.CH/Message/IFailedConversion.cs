namespace DSharpPlus.CH.Message;

public interface IFailedConversion
{
    public Task HandleErrorAsync(InvalidMessageConvertionError error, Entities.DiscordMessage message);
}
