namespace DSharpPlus
{
    public class VoiceServerUpdateEventArgs : DiscordEventArgs
    {
        /// <summary>
        /// Guild that got its voice server updated
        /// </summary>
        public DiscordGuild Guild { get; internal set; }

        /// <summary>
        /// New voice endpoint
        /// </summary>
        public string Endpoint { get; internal set; }
        internal string VoiceToken { get; set; }

        public VoiceServerUpdateEventArgs(DiscordClient client) : base(client) { }
    }
}
