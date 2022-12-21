using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DSharpPlus.Entities.Internal.Gateway.Payloads;

/// <summary>
/// Sent when the current user gains access to a channel.
/// </summary>
public sealed record InternalThreadListSyncPayload
{
    /// <summary>
    /// The id of the guild.
    /// </summary>
    [JsonPropertyName("guild_id")]
    public InternalSnowflake GuildId { get; init; } = null!;

    /// <summary>
    /// The parent channel ids whose threads are being synced. If omitted, then threads were synced for the entire guild. This array may contain channel_ids that have no active threads as well, so you know to clear that data.
    /// </summary>
    [JsonPropertyName("channel_ids")]
    public Optional<IReadOnlyList<InternalSnowflake>> ChannelIds { get; init; }

    /// <summary>
    /// All active threads in the given channels that the current user can access.
    /// </summary>
    [JsonPropertyName("threads")]
    public IReadOnlyList<InternalChannel> Threads { get; init; } = Array.Empty<InternalChannel>();

    /// <summary>
    /// All thread member objects from the synced threads for the current user, indicating which threads the current user has been added to.
    /// </summary>
    [JsonPropertyName("members")]
    public IReadOnlyList<InternalThreadMember> Members { get; init; } = Array.Empty<InternalThreadMember>();
}
