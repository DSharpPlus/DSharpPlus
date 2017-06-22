namespace DSharpPlus
{
    public class GuildRoleUpdateEventArgs : DiscordEventArgs
    {
        /// <summary>
        /// Guild that has its roles updated
        /// </summary>
        public DiscordGuild Guild { get; internal set; }
        /// <summary>
        /// New role list
        /// </summary>
        public DiscordRole RoleAfter { get; internal set; }
        /// <summary>
        /// Old role list
        /// </summary>
        public DiscordRole RoleBefore { get; internal set; }

        public GuildRoleUpdateEventArgs(DiscordClient client) : base(client) { }
    }
}
