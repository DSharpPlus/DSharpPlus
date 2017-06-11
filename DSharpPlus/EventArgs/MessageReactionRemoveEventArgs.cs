namespace DSharpPlus
{
    public class MessageReactionRemoveEventArgs : DiscordEventArgs
    {
        public ulong MessageId { get; internal set; }
        public DiscordChannel Channel { get; internal set; }
        public DiscordUser User { get; internal set; }
        public DiscordEmoji Emoji { get; internal set; }

        public MessageReactionRemoveEventArgs(DiscordClient client) : base(client) { }
    }
}
