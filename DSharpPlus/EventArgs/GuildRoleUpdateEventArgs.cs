namespace DSharpPlus
{
    public class GuildRoleUpdateEventArgs : DiscordEventArgs
    {
        public ulong GuildID { get; internal set; }
        public DiscordGuild Guild => this.Client.Guilds[GuildID];
        public DiscordRole Role { get; internal set; }
        public DiscordRole RoleBefore { get; internal set; }

        public GuildRoleUpdateEventArgs(DiscordClient client) : base(client) { }
    }
}
