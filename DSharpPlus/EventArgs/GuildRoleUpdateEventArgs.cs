namespace DSharpPlus
{
    public class GuildRoleUpdateEventArgs : DiscordEventArgs
    {
        public DiscordGuild Guild { get; internal set; }
        public DiscordRole RoleAfter { get; internal set; }
        public DiscordRole RoleBefore { get; internal set; }

        public GuildRoleUpdateEventArgs(DiscordClient client) : base(client) { }
    }
}
