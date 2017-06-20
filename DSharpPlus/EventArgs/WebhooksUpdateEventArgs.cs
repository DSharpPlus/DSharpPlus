namespace DSharpPlus
{
    public class WebhooksUpdateEventArgs : DiscordEventArgs
    {
        /// <summary>
        /// Guild that got its webhooks updated
        /// </summary>
        public DiscordGuild Guild { get; internal set; }
        /// <summary>
        /// Channel new webhook belongs to
        /// </summary>
        public DiscordChannel Channel { get; internal set; }

        public WebhooksUpdateEventArgs(DiscordClient client) : base(client) { }
    }
}
