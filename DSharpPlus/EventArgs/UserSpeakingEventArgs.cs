namespace DSharpPlus
{
    public class UserSpeakingEventArgs : DiscordEventArgs
    {
        /// <summary>
        /// User that started/stopped speaking
        /// </summary>
        public DiscordUser User { get; internal set; }
        public uint SSRC { get; internal set; }
        /// <summary>
        /// Whether this user is speaking
        /// </summary>
        public bool Speaking { get; internal set; }

        public UserSpeakingEventArgs(DiscordClient client) : base(client) { }
    }
}
