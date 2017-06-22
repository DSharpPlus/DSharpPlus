namespace DSharpPlus
{
    public class GuildRoleDeleteEventArgs : DiscordEventArgs
    {
        /// <summary>
        /// Guild that got a role deleted
        /// </summary>
        public DiscordGuild Guild { get; internal set; }
        /// <summary>
        /// Role that got deleted
        /// </summary>
        public DiscordRole Role { get; internal set; }

        public GuildRoleDeleteEventArgs(DiscordClient client) : base(client) { }
    }
}
