namespace DSharpPlus
{
    public class WebhooksUpdateEventArgs : DiscordEventArgs
    {
        public ulong GuildID { get; internal set; }
        public DiscordGuild Guild => this.Client.Guilds[GuildID];
        public ulong ChannelID { get; internal set; }
        public DiscordChannel Channel => this.Client.Guilds[GuildID].Channels.Find(x => x.Id == ChannelID);

        public WebhooksUpdateEventArgs(DiscordClient client) : base(client) { }
    }
}
