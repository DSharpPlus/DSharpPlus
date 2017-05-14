namespace DSharpPlus
{
    public class GuildMemberRemoveEventArgs : DiscordEventArgs
    {
        public ulong GuildID { get; internal set; }
        public DiscordGuild Guild => this.Client.Guilds[GuildID];
        public DiscordUser User { get; internal set; }

        public GuildMemberRemoveEventArgs(DiscordClient client) : base(client) { }
    }
}
