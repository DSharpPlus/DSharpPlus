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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Exceptions;
using DSharpPlus.Net.Abstractions;
using DSharpPlus.Net.Models;
using Newtonsoft.Json;

namespace DSharpPlus.Entities
{
    public class DiscordThreadChannelMetadata
    {
        /// <summary>
        /// Gets whether this thread is archived or not.
        /// </summary>
        [JsonProperty("archived", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsArchived { get; internal set; }

        /// <summary>
        /// Gets the duration in minutes to automatically archive the thread after recent activity. Can be set to: 60, 1440, 4320, 10080.
        /// </summary>
        [JsonProperty("auto_archive_duration", NullValueHandling = NullValueHandling.Ignore)]
        public AutoArchiveDuration AutoArchiveDuration { get; internal set; }

        /// <summary>
        /// Gets the time timestamp for when the thread's archive status was last changed.
        /// </summary>
        [JsonIgnore]
        public DateTimeOffset? ArchiveTimestamp
            => !string.IsNullOrWhiteSpace(this.ArchiveTimestampRaw) && DateTimeOffset.TryParse(this.ArchiveTimestampRaw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dto) ?
                dto : null;

        [JsonProperty("archive_timestamp", NullValueHandling = NullValueHandling.Ignore)]
        internal string ArchiveTimestampRaw { get; set; }

        /// <summary>
        /// Gets whether this thread is locked or not.
        /// </summary>
        [JsonProperty("locked", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsLocked { get; internal set; }

        internal DiscordThreadChannelMetadata() { }
    }
}
