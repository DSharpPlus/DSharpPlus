namespace DSharpPlus
{
    public class DmChannelDeleteEventArgs : DiscordEventArgs
    {
        public DiscordDmChannel Channel { get; internal set; }

        public DmChannelDeleteEventArgs(DiscordClient client) : base(client) { }
    }
}
