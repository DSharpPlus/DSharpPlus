namespace DSharpPlus
{
    public class VoiceStateUpdateEventArgs : DiscordEventArgs
    {
        /// <summary>
        /// user whose voice state was updated
        /// </summary>
        public DiscordUser User { get; internal set; }
        /// <summary>
        /// Guild whose user's voice state got updated
        /// </summary>
        public DiscordGuild Guild { get; internal set; }
        /// <summary>
        /// Voice channel
        /// </summary>
        public DiscordChannel Channel { get; internal set; }
        internal string SessionId { get; set; }

        public VoiceStateUpdateEventArgs(DiscordClient client) : base(client) { }
    }
}
