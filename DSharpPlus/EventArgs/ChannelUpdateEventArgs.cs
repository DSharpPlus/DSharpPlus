namespace DSharpPlus
{
    public class ChannelUpdateEventArgs : DiscordEventArgs
    {
        public DiscordChannel Channel { get; internal set; }
        public DiscordGuild Guild { get; internal set; }
        public DiscordChannel ChannelBefore { get; internal set; }

        public ChannelUpdateEventArgs(DiscordClient client) : base(client) { }
    }
}
