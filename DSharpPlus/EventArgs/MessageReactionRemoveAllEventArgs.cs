namespace DSharpPlus
{
    public class MessageReactionRemoveAllEventArgs : DiscordEventArgs
    {
        public ulong ChannelID { get; internal set; }
        public ulong MessageID { get; internal set; }
        public DiscordMessage Message => this.Client._rest_client.InternalGetMessage(ChannelID, MessageID).GetAwaiter().GetResult();
        public DiscordChannel Channel => this.Client._rest_client.InternalGetChannel(ChannelID).GetAwaiter().GetResult();

        public MessageReactionRemoveAllEventArgs(DiscordClient client) : base(client) { }
    }
}
