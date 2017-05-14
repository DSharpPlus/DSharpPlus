namespace DSharpPlus
{
    public class GuildIntegrationsUpdateEventArgs : DiscordEventArgs
    {
        internal ulong GuildID { get; set; }
        public DiscordGuild Guild => this.Client.Guilds[GuildID];

        public GuildIntegrationsUpdateEventArgs(DiscordClient client) : base(client) { }
    }
}
