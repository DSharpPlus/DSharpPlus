namespace DSharpPlus
{
    public class DmChannelCreateEventArgs : DiscordEventArgs
    {
        public DiscordDmChannel Channel { get; internal set; }

        public DmChannelCreateEventArgs(DiscordClient client) : base(client) { }
    }
}
