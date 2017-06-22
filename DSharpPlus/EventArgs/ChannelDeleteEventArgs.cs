namespace DSharpPlus
{
    public class ChannelDeleteEventArgs : DiscordEventArgs
    {
        /// <summary>
        /// Channel that just got deleted
        /// </summary>
        public DiscordChannel Channel { get; internal set; }
        /// <summary>
        /// Guild this channel belonged to
        /// </summary>
        public DiscordGuild Guild { get; internal set; }

        public ChannelDeleteEventArgs(DiscordClient client) : base(client) { }
    }
}
