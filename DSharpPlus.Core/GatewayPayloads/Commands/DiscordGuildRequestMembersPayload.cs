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

using System.Text.Json.Serialization;
using DSharpPlus.Core.Entities;

namespace DSharpPlus.Core.Gateway.Commands
{
    /// <summary>
    /// Used to request all members for a guild or a list of guilds.
    /// </summary>
    public sealed record DiscordGuildRequestMembersCommand
    {
        /// <summary>
        /// The id of the guild to get members for.
        /// </summary>
        /// <remarks>
        /// Always required.
        /// </remarks>
        [JsonPropertyName("guild_id")]
        public DiscordSnowflake GuildId { get; init; } = null!;

        /// <summary>
        /// The string that username starts with, or an empty string to return all members.
        /// </summary>
        /// <remarks>
        /// Required if <see cref="UserIds"/> is not set.
        /// </remarks>
        [JsonPropertyName("query")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Optional<string> Query { get; init; }

        /// <summary>
        /// The maximum number of members to send matching the query; a limit of 0 can be used with an empty string query to return all members.
        /// </summary>
        /// <remarks>
        /// Required when <see cref="Query"/> is set.
        /// </remarks>
        [JsonPropertyName("limit")]
        public int Limit { get; init; }

        /// <summary>
        /// Used to specify if we want the presences of the matched members.
        /// </summary>
        [JsonPropertyName("presences")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Optional<bool> Presences { get; init; }

        /// <summary>
        /// Used to specify which users you wish to fetch.
        /// </summary>
        /// <remarks>
        /// Required if <see cref="Query"/> is not set.
        /// </remarks>
        [JsonPropertyName("user_ids")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Optional<DiscordSnowflake[]> UserIds { get; init; }

        /// <summary>
        /// Nonce to identify the Guild Members Chunk response.
        /// </summary>
        /// <remarks>
        /// Nonce can only be up to 32 bytes. If you send an invalid nonce it will be ignored and the reply member_chunk(s) will not have a nonce set.
        /// </remarks>
        [JsonPropertyName("nonce")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Optional<string> Nonce { get; init; }
    }
}
