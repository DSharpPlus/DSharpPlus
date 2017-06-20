namespace DSharpPlus
{
    public class ChannelCreateEventArgs : DiscordEventArgs
    {
        /// <summary>
        /// Channel that just got created
        /// </summary>
        public DiscordChannel Channel { get; internal set; }
        /// <summary>
        /// Guild the newly created channel belongs to
        /// </summary>
        public DiscordGuild Guild { get; internal set; }

        public ChannelCreateEventArgs(DiscordClient client) : base(client) { }
    }
}
