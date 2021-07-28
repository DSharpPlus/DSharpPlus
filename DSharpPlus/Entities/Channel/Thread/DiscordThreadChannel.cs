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
    /// <summary>
    /// Represents a discord thread in a channel.
    /// </summary>
    public class DiscordThreadChannel : DiscordChannel
    {
        /// <summary>
        /// Gets the ID of this thread's creator.
        /// </summary>
        [JsonProperty("owner_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong OwnerId { get; internal set; }

        /// <summary>
        /// Gets the approximate count of messages in a thread, capped to 50.
        /// </summary>
        [JsonProperty("message_count", NullValueHandling = NullValueHandling.Ignore)]
        public int? MessageCount { get; internal set; }

        /// <summary>
        /// Gets the approximate count of members in a thread, capped to 50.
        /// </summary>
        [JsonProperty("member_count", NullValueHandling = NullValueHandling.Ignore)]
        public int? MemberCount { get; internal set; }

        /// <summary>
        /// Represents the current member for this thread. This will have a value if the user has joined the thread.
        /// </summary>
        [JsonProperty("member", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordThreadChannelMember> CurrentMember { get; internal set; }

        /// <summary>
        /// Gets the approximate count of members in a thread, up to 50.
        /// </summary>
        [JsonProperty("thread_metadata", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordThreadChannelMetadata ThreadMetadata { get; internal set; }

        //thread member
        #region Methods

        /// <summary>
        /// Makes the current user join the thread.
        /// </summary>
        public async Task JoinThreadAsync()
            => await this.Discord.ApiClient.JoinThreadAsync(this.Id);

        /// <summary>
        /// Makes the current user leave the thread.
        /// </summary>
        public async Task LeaveThreadAsync()
            => await this.Discord.ApiClient.LeaveThreadAsync(this.Id);

        /// <summary>
        /// Returns a full list of the thread members in this thread.
        /// </summary>
        /// <returns>A collection of all threads members in this thread.</returns>
        /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public async Task<IReadOnlyList<DiscordThreadChannelMember>> ListJoinedMembersAsync()
            => await this.Discord.ApiClient.ListThreadMembersAsync(this.Id);

        /// <summary>
        /// Adds the given DiscordMember to this thread. Requires an unarchived thread and send message permissions.
        /// </summary>
        /// <param name="member">The member to add to the thread.</param>
        /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public async Task AddThreadMemberAsync(DiscordMember member)
        {
            if (this.ThreadMetadata.IsArchived)
                throw new InvalidOperationException("You cannot add members to an archived thread.");
            //check for message send permissions
            await this.Discord.ApiClient.AddThreadMemberAsync(this.Id, member.Id);
        }

        /// <summary>
        /// Removes the given DiscordMember from this thread. Requires an unarchived thread and send message permissions.
        /// </summary>
        /// <param name="member">The member to remove from the thread.</param>
        /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public async Task RemoveThreadMemberAsync(DiscordMember member)
        {
            if (this.ThreadMetadata.IsArchived)
                throw new InvalidOperationException("You cannot remove members from an archived thread.");
            //check for message send permissions // Requires the MANAGE_THREADS permission, or the creator of the thread if it is a GUILD_PRIVATE_THREAD.
            await this.Discord.ApiClient.RemoveThreadMemberAsync(this.Id, member.Id);
        }

        #endregion

        internal DiscordThreadChannel() {}
    }
}
