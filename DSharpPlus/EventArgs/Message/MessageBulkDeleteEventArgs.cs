using System.Collections.Generic;
using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

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
    /// Gets the channel in which the deletion occurred. This value is null if the channel was not in cache.
    /// </summary>
    public DiscordChannel? Channel { get; internal set; }
    
    /// <summary>
    /// Gets the id of the channel in which the deletion occurred.
    /// </summary>
    public ulong ChannelId { get; internal set; }

    /// <summary>
    /// Gets the guild in which the deletion occurred. This value is null if the guild was not in cache.
    /// </summary>
    public DiscordGuild? Guild { get; internal set; }
    
    /// <summary>
    /// Gets the id of the guild in which the deletion occurred. This value is null if the the messages was not deleted in a guild.
    /// </summary>
    public ulong? GuildId { get; internal set; }

    internal MessageBulkDeleteEventArgs() : base() { }
}
