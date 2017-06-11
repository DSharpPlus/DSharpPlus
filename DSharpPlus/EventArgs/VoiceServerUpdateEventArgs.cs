namespace DSharpPlus
{
    public class VoiceServerUpdateEventArgs : DiscordEventArgs
    {
        public DiscordGuild Guild { get; internal set; }

        public string Endpoint { get; internal set; }
        internal string VoiceToken { get; set; }

        public VoiceServerUpdateEventArgs(DiscordClient client) : base(client) { }
    }
}
