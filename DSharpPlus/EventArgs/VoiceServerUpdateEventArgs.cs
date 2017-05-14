namespace DSharpPlus
{
    public class VoiceServerUpdateEventArgs : DiscordEventArgs
    {
        internal string VoiceToken { get; set; }
        public ulong GuildID { get; internal set; }
        public DiscordGuild Guild => this.Client.Guilds[GuildID];
        public string Endpoint { get; internal set; }

        public VoiceServerUpdateEventArgs(DiscordClient client) : base(client) { }
    }
}
