namespace DSharpPlus
{
    public class GuildRoleDeleteEventArgs : DiscordEventArgs
    {
        public ulong GuildID { get; internal set; }
        public DiscordGuild Guild => this.Client.Guilds[GuildID];
        public ulong RoleID { get; internal set; }

        public GuildRoleDeleteEventArgs(DiscordClient client) : base(client) { }
    }
}
