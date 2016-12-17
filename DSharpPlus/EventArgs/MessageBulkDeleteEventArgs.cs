using System.Collections.Generic;
using DSharpPlus.Objects;

namespace DSharpPlus.EventArgs
{
    public class MessageBulkDeleteEventArgs : System.EventArgs
    {
        public List<ulong> MessageIDs;
        public ulong ChannelID;
        public DiscordChannel Channel => DiscordClient.InternalGetChannel(ChannelID).Result;
    }
}
