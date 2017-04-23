namespace DSharpPlus
{
    public class MessageDeleteEventArgs : DiscordEventArgs
    {
        public ulong MessageID { get; internal set; }
        public ulong ChannelID { get; internal set; }
        public DiscordChannel Channel => this.Client._rest_client.InternalGetChannel(ChannelID).GetAwaiter().GetResult();

        public MessageDeleteEventArgs(DiscordClient client) : base(client) { }
    }
}
