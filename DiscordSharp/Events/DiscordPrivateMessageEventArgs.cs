namespace DiscordSharp
{
    public class DiscordPrivateMessageEventArgs
    {
        public DiscordPrivateChannel Channel { get; internal set; }
        public string username { get; internal set; }
        public string message { get; internal set; }
    }
}