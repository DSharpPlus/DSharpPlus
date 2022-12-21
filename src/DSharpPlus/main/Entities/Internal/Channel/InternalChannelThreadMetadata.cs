using System;
using System.Text.Json.Serialization;

namespace DSharpPlus.Entities.Internal;

/// <summary>
/// The thread metadata object contains a number of thread-specific channel fields that are not needed by other channel types.
/// </summary>
public sealed record InternalChannelThreadMetadata
{
    /// <summary>
    /// Whether the thread is archived.
    /// </summary>
    [JsonPropertyName("archived")]
    public bool Archived { get; init; }

    /// <summary>
    /// Duration in minutes to automatically archive the thread after recent activity, can be set to: 60, 1440, 4320, 10080.
    /// </summary>
    [JsonPropertyName("auto_archive_duration")]
    public DiscordThreadAutoArchiveDuration AutoArchiveDuration { get; init; }

    /// <summary>
    /// Timestamp when the thread's archive status was last changed, used for calculating recent activity.
    /// </summary>
    [JsonPropertyName("archive_timestamp")]
    public DateTimeOffset ArchiveTimestamp { get; init; }

    /// <summary>
    /// Whether the thread is locked; when a thread is locked, only users with <see cref="DiscordPermissions.ManageThreads"/> can unarchive it.
    /// </summary>
    [JsonPropertyName("locked")]
    public bool Locked { get; init; }

    /// <summary>
    /// Whether non-moderators can add other non-moderators to a thread; only available on private threads.
    /// </summary>
    [JsonPropertyName("invitable")]
    public Optional<bool> Invitable { get; init; }

    /// <summary>
    /// Timestamp when the thread was created; only populated for threads created after 2022-01-09
    /// </summary>
    [JsonPropertyName("create_timestamp")]
    public Optional<DateTimeOffset> CreateTimestamp { get; init; }
}
