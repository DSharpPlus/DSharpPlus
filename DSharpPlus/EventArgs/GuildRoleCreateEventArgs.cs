namespace DSharpPlus
{
    public class GuildRoleCreateEventArgs : DiscordEventArgs
    {
        public ulong GuildID { get; internal set; }
        public DiscordGuild Guild => this.Client.Guilds[GuildID];
        public DiscordRole Role { get; internal set; }

        public GuildRoleCreateEventArgs(DiscordClient client) : base(client) { }
    }
}
