using System;
using System.Collections.Generic;

namespace DSharpPlus
{
    public class MessageBulkDeleteEventArgs : EventArgs
    {
        public IReadOnlyList<ulong> MessageIDs { get; internal set; }
        public ulong ChannelID { get; internal set; }
        public DiscordChannel Channel => DiscordClient.InternalGetChannel(ChannelID).Result;
    }
}
