using System;
using DSharpPlus.Core.Enums;
using Newtonsoft.Json;

namespace DSharpPlus.Core.RestEntities
{
    /// <summary>
    /// The thread metadata object contains a number of thread-specific channel fields that are not needed by other channel types.
    /// </summary>
    public sealed record DiscordChannelThreadMetadata
    {
        /// <summary>
        /// Whether the thread is archived.
        /// </summary>
        [JsonProperty("archived", NullValueHandling = NullValueHandling.Ignore)]
        public bool Archived { get; init; }

        /// <summary>
        /// Duration in minutes to automatically archive the thread after recent activity, can be set to: 60, 1440, 4320, 10080.
        /// </summary>
        [JsonProperty("auto_archive_duration", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordThreadAutoArchiveDuration AutoArchiveDuration { get; init; }

        /// <summary>
        /// Timestamp when the thread's archive status was last changed, used for calculating recent activity.
        /// </summary>
        [JsonProperty("archive_timestamp", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset ArchiveTimestamp { get; init; }

        /// <summary>
        /// Whether the thread is locked; when a thread is locked, only users with <see cref="DiscordPermissions.ManageThreads"/> can unarchive it.
        /// </summary>
        [JsonProperty("locked", NullValueHandling = NullValueHandling.Ignore)]
        public bool Locked { get; init; }

        /// <summary>
        /// Whether non-moderators can add other non-moderators to a thread; only available on private threads.
        /// </summary>
        [JsonProperty("invitable", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<bool> Invitable { get; init; }

        /// <summary>
        /// Timestamp when the thread was created; only populated for threads created after 2022-01-09
        /// </summary>
        [JsonProperty("create_timestamp", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DateTimeOffset> CreateTimestamp { get; init; }
    }
}
