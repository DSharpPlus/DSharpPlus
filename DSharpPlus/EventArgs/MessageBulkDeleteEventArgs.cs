using System;
using System.Collections.Generic;

namespace DSharpPlus
{
    public class MessageBulkDeleteEventArgs : EventArgs
    {
        public List<ulong> MessageIDs;
        public ulong ChannelID;
        public DiscordChannel Channel => DiscordClient.InternalGetChannel(ChannelID).Result;
    }
}
