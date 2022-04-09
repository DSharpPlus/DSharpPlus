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
using System.Text.Json.Serialization;

namespace DSharpPlus.Core.Entities
{
    /// <summary>
    /// A thread member is used to indicate whether a user has joined a thread or not.
    /// </summary>
    /// <remarks>
    /// The <see cref="Id"/> and <see cref="UserId"/> fields are omitted on the member sent within each thread in the <c>GUILD_CREATE</c> event
    /// </remarks>
    public sealed record DiscordThreadMember
    {
        /// <summary>
        /// The id of the thread.
        /// </summary>
        [JsonPropertyName("id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Optional<DiscordSnowflake> Id { get; init; }

        /// <summary>
        /// The id of the user.
        /// </summary>
        [JsonPropertyName("user_id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Optional<DiscordSnowflake> UserId { get; init; }

        /// <summary>
        /// The time the current user last joined the thread.
        /// </summary>
        [JsonPropertyName("join_timestamp")]
        public DateTimeOffset JoinTimestamp { get; init; }

        /// <summary>
        /// Any user-thread settings, currently only used for notifications.
        /// </summary>
        [JsonPropertyName("flags")]
        public int Flags { get; init; }

        /// <summary>
        /// The id of the guild.
        /// </summary>
        /// <remarks>
        /// Only sent on the ThreadMemberUpdate gateway payload.
        /// </remarks>
        [JsonPropertyName("guild_id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Optional<DiscordSnowflake> GuildId { get; init; }
    }
}
