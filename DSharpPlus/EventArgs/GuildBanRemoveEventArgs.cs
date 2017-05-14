namespace DSharpPlus
{
    public class GuildBanRemoveEventArgs : DiscordEventArgs
    {
        public DiscordUser User { get; internal set; }
        public ulong GuildID { get; internal set; }
        public DiscordGuild Guild => this.Client.Guilds[GuildID];

        public GuildBanRemoveEventArgs(DiscordClient client) : base(client) { }
    }
}
