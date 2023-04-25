namespace DSharpPlus.CH.Message
{
    interface IFailedConvertion
    {
        public Task HandleErrorAsync(InvalidMessageConvertionError error, DSharpPlus.Entities.DiscordMessage message);
    }
}