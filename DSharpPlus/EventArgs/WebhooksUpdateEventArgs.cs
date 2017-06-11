namespace DSharpPlus
{
    public class WebhooksUpdateEventArgs : DiscordEventArgs
    {
        public DiscordGuild Guild { get; internal set; }
        public DiscordChannel Channel { get; internal set; }

        public WebhooksUpdateEventArgs(DiscordClient client) : base(client) { }
    }
}
