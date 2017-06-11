namespace DSharpPlus
{
    public class GuildRoleDeleteEventArgs : DiscordEventArgs
    {
        public DiscordGuild Guild { get; internal set; }
        public DiscordRole Role { get; internal set; }

        public GuildRoleDeleteEventArgs(DiscordClient client) : base(client) { }
    }
}
