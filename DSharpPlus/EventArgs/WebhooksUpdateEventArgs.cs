namespace DSharpPlus
{
    public class WebhooksUpdateEventArgs : DiscordEventArgs
    {
        public ulong GuildID { get; internal set; }
        public DiscordGuild Guild => this.Client._rest_client.InternalGetGuildAsync(GuildID).GetAwaiter().GetResult();
        public ulong ChannelID { get; internal set; }
        public DiscordChannel Channel => this.Client._rest_client.InternalGetChannel(ChannelID).GetAwaiter().GetResult();

        public WebhooksUpdateEventArgs(DiscordClient client) : base(client) { }
    }
}
