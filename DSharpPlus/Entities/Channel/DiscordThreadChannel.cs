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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Exceptions;
using DSharpPlus.Net.Abstractions;
using DSharpPlus.Net.Models;
using DSharpPlus.Net.Serialization;
using Newtonsoft.Json;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents a discord thread channel.
    /// </summary>
    public class DiscordThreadChannel : SnowflakeObject, IEquatable<DiscordThreadChannel>
    {
        /// <summary>
        /// Gets ID of the guild to which this thread belongs.
        /// </summary>
        [JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong GuildId { get; internal set; }

        /// <summary>
        /// Gets ID of the owner that started this thread.
        /// </summary>
        [JsonProperty("owner_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong OwnerId { get; internal set; }

        /// <summary>
        /// Gets ID of the news or text channel that contains this thread.
        /// </summary>
        [JsonProperty("parent_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong ParentChannelId { get; internal set; }

        /// <summary>
        /// Gets the name of this thread.
        /// </summary>
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; internal set; }

        /// <summary>
        /// Gets the type of this thread.
        /// </summary>
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public ChannelType Type { get; internal set; }

        /// <summary>
        /// Gets whether this thread is private.
        /// </summary>
        [JsonIgnore]
        public bool IsPrivate
            => this.Type == ChannelType.PrivateThread;

        /// <summary>
        /// Gets the ID of the last message sent in this thread.
        /// </summary>
        [JsonProperty("last_message_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong? LastMessageId { get; internal set; }

        /// <summary>
        /// <para>Gets the slow mode delay configured for this thread.</para>
        /// <para>All bots, as well as users with <see cref="Permissions.ManageChannels"/> or <see cref="Permissions.ManageMessages"/> permissions in the channel are exempt from slow mode.</para>
        /// </summary>
        [JsonProperty("rate_limit_per_user", NullValueHandling = NullValueHandling.Ignore)]
        public int? PerUserRateLimit { get; internal set; }

        /// <summary>
        /// Gets when the last pinned message was pinned in this thread.
        /// </summary>
        [JsonIgnore]
        public DateTimeOffset? LastPinTimestamp
            => !string.IsNullOrWhiteSpace(this.LastPinTimestampRaw) && DateTimeOffset.TryParse(this.LastPinTimestampRaw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dto) ?
                dto : null;

        [JsonProperty("last_pin_timestamp", NullValueHandling = NullValueHandling.Ignore)]
        internal string LastPinTimestampRaw { get; set; }

        /// <summary>
        /// Gets the threads metadata.
        /// </summary>
        [JsonProperty("thread_metadata", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordThreadChannelMetadata ThreadMetadata { get; internal set; }

        /// <summary>
        /// Gets the thread member object.
        /// </summary>
        [JsonIgnore]
        public IReadOnlyDictionary<ulong, DiscordThreadChannelMember> ThreadMembers => new ReadOnlyConcurrentDictionary<ulong, DiscordThreadChannelMember>(this._threadMembers);

        [JsonProperty("thread_member", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(SnowflakeArrayAsDictionaryJsonConverter))]
        internal ConcurrentDictionary<ulong, DiscordThreadChannelMember> _threadMembers;

        #region Methods

        /// <summary>
        /// Deletes a thread
        /// </summary>
        /// <param name="reason">Reason for audit logs.</param>
        /// <returns></returns>
        /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageThreads"/> permission.</exception>
        /// <exception cref="Exceptions.NotFoundException">Thrown when the channel does not exist.</exception>
        /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task DeleteAsync(string reason = null)
            => this.Discord.ApiClient.DeleteThreadAsync(this.Id, reason);


        /// <summary>
        /// Modifies the current thread.
        /// </summary>
        /// <param name="action">Action to perform on this thread</param>
        /// <returns></returns>
        /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageThreads"/> permission.</exception>
        /// <exception cref="Exceptions.NotFoundException">Thrown when the channel does not exist.</exception>
        /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception
        public Task ModifyAsync(Action<ThreadEditModel> action)
        {
            var mdl = new ThreadEditModel();
            action(mdl);
            return this.Discord.ApiClient.ModifyThreadAsync(this.Id, mdl.Name, mdl.Locked, mdl.Archived, mdl.AutoArchiveDuration, mdl.PerUserRateLimit, mdl.AuditLogReason);
        }

        /// <summary>
        /// Returns a string representation of this thread.
        /// </summary>
        /// <returns>String representation of this thread.</returns>
        public override string ToString()
        {
            var threadchannel = (object)this.Type switch
            {
                ChannelType.NewsThread => $"News thread {this.Name} ({this.Id})",
                ChannelType.PublicThread => $"Thread {this.Name} ({this.Id})",
                ChannelType.PrivateThread => $"Private thread {this.Name} ({this.Id})",
                _ => $"Thread {this.Name} ({this.Id})",
            };
            return threadchannel;
        }
        #endregion

        /// <summary>
        /// Checks whether this <see cref="DiscordThreadChannel"/> is equal to another object.
        /// </summary>
        /// <param name="obj">Object to compare to.</param>
        /// <returns>Whether the object is equal to this <see cref="DiscordThreadChannel"/>.</returns>
        public override bool Equals(object obj) => this.Equals(obj as DiscordThreadChannel);

        /// <summary>
        /// Checks whether this <see cref="DiscordThreadChannel"/> is equal to another <see cref="DiscordThreadChannel"/>.
        /// </summary>
        /// <param name="e"><see cref="DiscordThreadChannel"/> to compare to.</param>
        /// <returns>Whether the <see cref="DiscordThreadChannel"/> is equal to this <see cref="DiscordThreadChannel"/>.</returns>
        public bool Equals(DiscordThreadChannel e)
        {
            if (e is null)
                return false;

            return ReferenceEquals(this, e) ? true : this.Id == e.Id;
        }

        /// <summary>
        /// Gets the hash code for this <see cref="DiscordThreadChannel"/>.
        /// </summary>
        /// <returns>The hash code for this <see cref="DiscordThreadChannel"/>.</returns>
        public override int GetHashCode() => this.Id.GetHashCode();

        /// <summary>
        /// Gets whether the two <see cref="DiscordThreadChannel"/> objects are equal.
        /// </summary>
        /// <param name="e1">First channel to compare.</param>
        /// <param name="e2">Second channel to compare.</param>
        /// <returns>Whether the two channels are equal.</returns>
        public static bool operator ==(DiscordThreadChannel e1, DiscordThreadChannel e2)
        {
            var o1 = e1 as object;
            var o2 = e2 as object;

            if ((o1 == null && o2 != null) || (o1 != null && o2 == null))
                return false;

            return o1 == null && o2 == null ? true : e1.Id == e2.Id;
        }

        /// <summary>
        /// Gets whether the two <see cref="DiscordThreadChannel"/> objects are not equal.
        /// </summary>
        /// <param name="e1">First channel to compare.</param>
        /// <param name="e2">Second channel to compare.</param>
        /// <returns>Whether the two channels are not equal.</returns>
        public static bool operator !=(DiscordThreadChannel e1, DiscordThreadChannel e2)
            => !(e1 == e2);
    }
}
