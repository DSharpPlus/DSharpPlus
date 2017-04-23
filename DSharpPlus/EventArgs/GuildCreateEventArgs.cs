namespace DSharpPlus
{
    public class GuildCreateEventArgs : DiscordEventArgs
    {
        public DiscordGuild Guild { get; internal set; }

        public GuildCreateEventArgs(DiscordClient client) : base(client) { }
    }
}
