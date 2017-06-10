namespace DSharpPlus
{
    public class GuildDeleteEventArgs : DiscordEventArgs
    {
        public DiscordGuild Guild { get; internal set; }
        public bool Unavailable { get; internal set; }

        public GuildDeleteEventArgs(DiscordClient client) : base(client) { }
    }
}
