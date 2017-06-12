using System.Collections.Generic;

namespace DSharpPlus
{
    public class MessageBulkDeleteEventArgs : DiscordEventArgs
    {
        public IReadOnlyList<DiscordMessage> Messages { get; internal set; }
        public DiscordChannel Channel { get; internal set; }

        public MessageBulkDeleteEventArgs(DiscordClient client) : base(client) { }
    }
}
