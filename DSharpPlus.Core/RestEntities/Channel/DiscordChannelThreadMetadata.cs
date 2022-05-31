// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2022 DSharpPlus Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

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
