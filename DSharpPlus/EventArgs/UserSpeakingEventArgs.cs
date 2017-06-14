namespace DSharpPlus
{
    public class UserSpeakingEventArgs : DiscordEventArgs
    {
        public DiscordUser User { get; internal set; }
        public uint SSRC { get; internal set; }
        public bool Speaking { get; internal set; }

        public UserSpeakingEventArgs(DiscordClient client) : base(client) { }
    }
}
