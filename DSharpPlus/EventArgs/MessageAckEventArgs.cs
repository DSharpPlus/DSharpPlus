namespace DSharpPlus
{
    public sealed class MessageAckEventArgs : DiscordEventArgs
    {
        public DiscordMessage Message { get; internal set; }
        public DiscordChannel Channel => this.Message.Channel;

        public MessageAckEventArgs(DiscordClient client) : base(client) { }
    }
}
