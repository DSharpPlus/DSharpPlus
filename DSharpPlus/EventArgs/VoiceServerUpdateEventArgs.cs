namespace DSharpPlus
{
    public class VoiceServerUpdateEventArgs : DiscordEventArgs
    {
        internal string VoiceToken { get; set; }
        public ulong GuildID { get; internal set; }
        public DiscordGuild Guild => this.Client._rest_client.InternalGetGuildAsync(GuildID).GetAwaiter().GetResult();
        public string Endpoint { get; internal set; }

        public VoiceServerUpdateEventArgs(DiscordClient client) : base(client) { }
    }
}
