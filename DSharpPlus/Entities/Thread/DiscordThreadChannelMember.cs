// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2021 DSharpPlus Contributors
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
using System.Globalization;
using Newtonsoft.Json;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents a discord thread member object.
    /// </summary>
    public class DiscordThreadChannelMember : SnowflakeObject, IEquatable<DiscordThreadChannelMember>
    {
        /// <summary>
        /// Gets the id of the user.
        /// </summary>
        [JsonProperty("user_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong UserId { get; internal set; }

        /// <summary>
        /// Gets the id of the user.
        /// </summary>
        [JsonProperty("member", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordMember Member { get; internal set; }

        /// <summary>
        /// Gets the id of the user.
        /// </summary>
        [JsonProperty("presence", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordPresence Presence { get; internal set; }

        /// <summary>
        /// Gets the timestamp when the user joined the thread.
        /// </summary>
        [JsonIgnore]
        public DateTimeOffset? JoinTimeStamp
            => !string.IsNullOrWhiteSpace(this.JoinTimeStampRaw) && DateTimeOffset.TryParse(this.JoinTimeStampRaw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dto) ?
                dto : null;

        /// <summary>
        /// Gets the timestamp when the user joined the thread as raw string.
        /// </summary>
        [JsonProperty("join_timestamp", NullValueHandling = NullValueHandling.Ignore)]
        internal string JoinTimeStampRaw { get; set; }

        /// <summary>
        /// Gets flags (Currently only for notification settings).
        /// </summary>
        [JsonProperty("flags", NullValueHandling = NullValueHandling.Ignore)]
        public ThreadNotificationSetting Flags { get; internal set; }


        /// <summary>
        /// Checks whether this <see cref="DiscordThreadChannelMember"/> is equal to another object.
        /// </summary>
        /// <param name="obj">Object to compare to.</param>
        /// <returns>Whether the object is equal to this <see cref="DiscordThreadChannelMember"/>.</returns>
        public override bool Equals(object obj) => this.Equals(obj as DiscordThreadChannelMember);

        /// <summary>
        /// Checks whether this <see cref="DiscordThreadChannel"/> is equal to another <see cref="DiscordThreadChannelMember"/>.
        /// </summary>
        /// <param name="e"><see cref="DiscordThreadChannel"/> to compare to.</param>
        /// <returns>Whether the <see cref="DiscordThreadChannel"/> is equal to this <see cref="DiscordThreadChannelMember"/>.</returns>
        public bool Equals(DiscordThreadChannelMember e) => e is not null && (ReferenceEquals(this, e) || this.Id == e.Id);

        /// <summary>
        /// Gets the hash code for this <see cref="DiscordThreadChannelMember"/>.
        /// </summary>
        /// <returns>The hash code for this <see cref="DiscordThreadChannelMember"/>.</returns>
        public override int GetHashCode() => this.Id.GetHashCode();

        /// <summary>
        /// Gets whether the two <see cref="DiscordThreadChannel"/> objects are equal.
        /// </summary>
        /// <param name="e1">First channel to compare.</param>
        /// <param name="e2">Second channel to compare.</param>
        /// <returns>Whether the two channels are equal.</returns>
        public static bool operator ==(DiscordThreadChannelMember e1, DiscordThreadChannelMember e2)
        {
            var o1 = e1 as object;
            var o2 = e2 as object;

            return (o1 != null || o2 == null) && (o1 == null || o2 != null) && ((o1 == null && o2 == null) || e1.Id == e2.Id);
        }

        /// <summary>
        /// Gets whether the two <see cref="DiscordThreadChannelMember"/> objects are not equal.
        /// </summary>
        /// <param name="e1">First channel to compare.</param>
        /// <param name="e2">Second channel to compare.</param>
        /// <returns>Whether the two channels are not equal.</returns>
        public static bool operator !=(DiscordThreadChannelMember e1, DiscordThreadChannelMember e2)
            => !(e1 == e2);

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscordThreadChannelMember"/> class.
        /// </summary>
        internal DiscordThreadChannelMember() { }
    }
}
