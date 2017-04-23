namespace DSharpPlus
{
    public class MessageReactionRemoveEventArgs : DiscordEventArgs
    {
        public ulong UserID { get; internal set; }
        public ulong MessageID { get; internal set; }
        public ulong ChannelID { get; internal set; }
        public DiscordEmoji Emoji { get; internal set; }
        public DiscordUser User => this.Client._rest_client.InternalGetUser(UserID).GetAwaiter().GetResult();
        public DiscordMessage Message => this.Client._rest_client.InternalGetMessage(ChannelID, MessageID).GetAwaiter().GetResult();
        public DiscordChannel Channel => this.Client._rest_client.InternalGetChannel(ChannelID).GetAwaiter().GetResult();

        public MessageReactionRemoveEventArgs(DiscordClient client) : base(client) { }
    }
}
