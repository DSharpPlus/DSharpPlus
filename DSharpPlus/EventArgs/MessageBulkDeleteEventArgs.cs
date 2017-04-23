using System;
using System.Collections.Generic;

namespace DSharpPlus
{
    public class MessageBulkDeleteEventArgs : EventArgs
    {
        internal DiscordClient Discord { get; set; }
        public IReadOnlyList<ulong> MessageIDs { get; internal set; }
        public ulong ChannelID { get; internal set; }
        public DiscordChannel Channel => this.Discord._rest_client.InternalGetChannel(ChannelID).GetAwaiter().GetResult();
    }
}
