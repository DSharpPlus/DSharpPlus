namespace DSharpPlus
{
    public class GuildMemberAddEventArgs : DiscordEventArgs
    {
        public DiscordMember Member { get; internal set; }
        public ulong GuildID { get; internal set; }
        public DiscordGuild Guild => this.Client.Guilds[GuildID];

        public GuildMemberAddEventArgs(DiscordClient client) : base(client) { }
    }
}
