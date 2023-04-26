namespace DSharpPlus.CH.Message;

public interface IFailedConvertion
{
    public Task HandleErrorAsync(InvalidMessageConvertionError error, Entities.DiscordMessage message);
}
