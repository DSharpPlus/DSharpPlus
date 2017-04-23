namespace DSharpPlus
{
    public class TypingStartEventArgs : DiscordEventArgs
    {
        public ulong ChannelID { get; internal set; }
        public ulong UserID { get; internal set; }
        public DiscordChannel Channel => this.Client._rest_client.InternalGetChannel(ChannelID).GetAwaiter().GetResult();
        public DiscordUser User => this.Client._rest_client.InternalGetUser(UserID.ToString()).GetAwaiter().GetResult();

        public TypingStartEventArgs(DiscordClient client) : base(client) { }
    }
}
