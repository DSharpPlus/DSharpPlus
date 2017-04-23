namespace DSharpPlus
{
    public class GuildBanRemoveEventArgs : DiscordEventArgs
    {
        public DiscordUser User { get; internal set; }
        public ulong GuildID { get; internal set; }
        public DiscordGuild Guild => this.Client._rest_client.InternalGetGuildAsync(GuildID).GetAwaiter().GetResult();

        public GuildBanRemoveEventArgs(DiscordClient client) : base(client) { }
    }
}
