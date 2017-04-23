namespace DSharpPlus
{
    public class ChannelDeleteEventArgs : DiscordEventArgs
    {
        public DiscordChannel Channel { get; internal set; }
        public DiscordGuild Guild { get; internal set; }

        public ChannelDeleteEventArgs(DiscordClient client) : base(client) { }
    }
}
