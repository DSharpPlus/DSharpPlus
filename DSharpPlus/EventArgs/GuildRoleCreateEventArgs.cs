namespace DSharpPlus
{
    public class GuildRoleCreateEventArgs : DiscordEventArgs
    {
        public DiscordGuild Guild { get; internal set; }
        public DiscordRole Role { get; internal set; }

        public GuildRoleCreateEventArgs(DiscordClient client) : base(client) { }
    }
}
