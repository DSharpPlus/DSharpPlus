namespace DSharpPlus
{
    public class GuildIntegrationsUpdateEventArgs : DiscordEventArgs
    {
        internal ulong GuildID { get; set; }
        public DiscordGuild Guild => this.Client._rest_client.InternalGetGuildAsync(GuildID).GetAwaiter().GetResult();

        public GuildIntegrationsUpdateEventArgs(DiscordClient client) : base(client) { }
    }
}
