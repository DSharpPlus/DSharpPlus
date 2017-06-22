namespace DSharpPlus
{
    public class DmChannelCreateEventArgs : DiscordEventArgs
    {
        /// <summary>
        /// DM Channel that just got created
        /// </summary>
        public DiscordDmChannel Channel { get; internal set; }

        public DmChannelCreateEventArgs(DiscordClient client) : base(client) { }
    }
}
