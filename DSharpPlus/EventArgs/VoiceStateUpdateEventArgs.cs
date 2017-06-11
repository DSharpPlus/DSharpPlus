namespace DSharpPlus
{
    public class VoiceStateUpdateEventArgs : DiscordEventArgs
    {
        public DiscordUser User { get; internal set; }
        public DiscordGuild Guild { get; internal set; }
        public DiscordChannel Channel { get; internal set; }
        internal string SessionId { get; set; }

        public VoiceStateUpdateEventArgs(DiscordClient client) : base(client) { }
    }
}
