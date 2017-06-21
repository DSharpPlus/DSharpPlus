namespace DSharpPlus
{
    public class ChannelUpdateEventArgs : DiscordEventArgs
    {
        /// <summary>
        /// Channel after it got updated
        /// </summary>
        public DiscordChannel ChannelAfter { get; internal set; }
        /// <summary>
        /// Guild that just got a channel updated
        /// </summary>
        public DiscordGuild Guild { get; internal set; }
        /// <summary>
        /// Channel before it got updated
        /// </summary>
        public DiscordChannel ChannelBefore { get; internal set; }

        public ChannelUpdateEventArgs(DiscordClient client) : base(client) { }
    }
}
