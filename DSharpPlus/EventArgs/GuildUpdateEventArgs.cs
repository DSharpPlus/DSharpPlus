namespace DSharpPlus
{
    public class GuildUpdateEventArgs : DiscordEventArgs
    {
        public DiscordGuild Guild { get; internal set; }

        public GuildUpdateEventArgs(DiscordClient client) : base(client) { }
    }
}
