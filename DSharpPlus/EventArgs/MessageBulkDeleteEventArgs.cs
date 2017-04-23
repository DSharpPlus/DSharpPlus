using System.Collections.Generic;

namespace DSharpPlus
{
    public class MessageBulkDeleteEventArgs : DiscordEventArgs
    {
        public IReadOnlyList<ulong> MessageIDs { get; internal set; }
        public ulong ChannelID { get; internal set; }
        public DiscordChannel Channel => this.Client._rest_client.InternalGetChannel(ChannelID).GetAwaiter().GetResult();

        public MessageBulkDeleteEventArgs(DiscordClient client) : base(client) { }
    }
}
