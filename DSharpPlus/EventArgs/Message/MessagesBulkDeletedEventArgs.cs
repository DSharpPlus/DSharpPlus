using System.Collections.Generic;
using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.MessagesBulkDeleted"/> event.
/// </summary>
public class MessagesBulkDeletedEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Gets a collection of the deleted messages.
    /// </summary>
    public IReadOnlyList<DiscordMessage> Messages { get; internal set; }

    /// <summary>
    /// Gets the channel in which the deletion occurred.
    /// </summary>
    public DiscordChannel Channel { get; internal set; }

    /// <summary>
    /// Gets the guild in which the deletion occurred.
    /// </summary>
    public DiscordGuild Guild { get; internal set; }

    internal MessagesBulkDeletedEventArgs() : base() { }
}
