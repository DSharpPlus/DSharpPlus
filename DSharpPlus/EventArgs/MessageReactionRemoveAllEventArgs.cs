namespace DSharpPlus
{
    public class MessageReactionRemoveAllEventArgs : DiscordEventArgs
    {
        public ulong MessageId { get; internal set; }
        public DiscordChannel Channel { get; internal set; }

        public MessageReactionRemoveAllEventArgs(DiscordClient client) : base(client) { }
    }
}
