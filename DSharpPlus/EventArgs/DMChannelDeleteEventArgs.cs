namespace DSharpPlus
{
    public class DmChannelDeleteEventArgs : DiscordEventArgs
    {
        /// <summary>
        /// DM Channel that just got deleted
        /// </summary>
        public DiscordDmChannel Channel { get; internal set; }

        public DmChannelDeleteEventArgs(DiscordClient client) : base(client) { }
    }
}
