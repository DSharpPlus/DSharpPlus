namespace DSharpPlus
{
    public class GuildIntegrationsUpdateEventArgs : DiscordEventArgs
    {
        public DiscordGuild Guild { get; internal set; }

        public GuildIntegrationsUpdateEventArgs(DiscordClient client) : base(client) { }
    }
}
