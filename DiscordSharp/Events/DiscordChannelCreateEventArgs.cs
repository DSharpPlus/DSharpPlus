namespace DiscordSharp
{
    public enum DiscordChannelCreateType
    {
        PRIVATE, CHANNEL
    }
    public class DiscordChannelCreateEventArgs
    {
        public DiscordChannelCreateType ChannelType { get; set; }
        public DiscordChannel ChannelCreated { get; set; }
    }
}