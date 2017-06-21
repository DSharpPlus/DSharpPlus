namespace DSharpPlus
{
    public class GuildIntegrationsUpdateEventArgs : DiscordEventArgs
    {
        /// <summary>
        /// Guild that just got its integrations updated
        /// </summary>
        public DiscordGuild Guild { get; internal set; }

        public GuildIntegrationsUpdateEventArgs(DiscordClient client) : base(client) { }
    }
}
