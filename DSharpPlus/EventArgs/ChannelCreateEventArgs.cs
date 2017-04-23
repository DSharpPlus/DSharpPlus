namespace DSharpPlus
{
    public class ChannelCreateEventArgs : DiscordEventArgs
    {
        public DiscordChannel Channel { get; internal set; }
        public DiscordGuild Guild { get; internal set; }

        public ChannelCreateEventArgs(DiscordClient client) : base(client) { }
    }
}
