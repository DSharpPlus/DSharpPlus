using System.Collections.Generic;
using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs
{
    /// <summary>
    /// Represents arguments for <see cref="DiscordClient.MessagesBulkDeleted"/> event.
    /// </summary>
    public class MessageBulkDeleteEventArgs : DiscordEventArgs
    {
        /// <summary>
        /// Gets a collection of the deleted messages.
        /// </summary>
        public IReadOnlyList<DiscordMessage> Messages { get; internal set; }

        /// <summary>
        /// Gets the channel in which the deletion occured.
        /// </summary>
        public DiscordChannel Channel { get; internal set; }

        internal MessageBulkDeleteEventArgs(DiscordClient client) : base(client) { }
    }
}
