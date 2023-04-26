namespace DSharpPlus.CH.Message
{
    public interface IFailedConvertion
    {
        public Task HandleErrorAsync(InvalidMessageConvertionError error, DSharpPlus.Entities.DiscordMessage message);
    }
}