namespace DSharpPlus
{
    public class GuildBanAddEventArgs : DiscordEventArgs
    {
        public DiscordUser User { get; internal set; }
        public ulong GuildID { get; internal set; }
        public DiscordGuild Guild => this.Client.Guilds[GuildID];

        public GuildBanAddEventArgs(DiscordClient client) : base(client) { }
    }
}
