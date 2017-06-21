namespace DSharpPlus
{
    public class GuildRoleCreateEventArgs : DiscordEventArgs
    {
        /// <summary>
        /// Guild that got a new role
        /// </summary>
        public DiscordGuild Guild { get; internal set; }
        /// <summary>
        /// Role that got created
        /// </summary>
        public DiscordRole Role { get; internal set; }

        public GuildRoleCreateEventArgs(DiscordClient client) : base(client) { }
    }
}
