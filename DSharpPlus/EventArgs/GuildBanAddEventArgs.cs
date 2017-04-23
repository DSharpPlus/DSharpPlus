namespace DSharpPlus
{
    public class GuildBanAddEventArgs : DiscordEventArgs
    {
        public DiscordUser User { get; internal set; }
        public ulong GuildID { get; internal set; }
        public DiscordGuild Guild => this.Client._rest_client.InternalGetGuildAsync(GuildID).GetAwaiter().GetResult();

        public GuildBanAddEventArgs(DiscordClient client) : base(client) { }
    }
}
