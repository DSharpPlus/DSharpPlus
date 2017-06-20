using System.Collections.Generic;

namespace DSharpPlus
{
    public class MessageBulkDeleteEventArgs : DiscordEventArgs
    {
        /// <summary>
        /// Messages that got deleted
        /// </summary>
        public IReadOnlyList<DiscordMessage> Messages { get; internal set; }
        /// <summary>
        /// Channel that had its messages deleted
        /// </summary>
        public DiscordChannel Channel { get; internal set; }

        public MessageBulkDeleteEventArgs(DiscordClient client) : base(client) { }
    }
}
