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

namespace DSharpPlus.Core.Gateway.Payloads
{
    /// <summary>
    /// Sent in response to Guild Request Members. You can use the chunk_index and chunk_count to calculate how many chunks are left for your request.
    /// </summary>
    public sealed record DiscordGuildMembersChunkPayload
    {
        /// <summary>
        /// The id of the guild.
        /// </summary>
        [JsonPropertyName("guild_id")]
        public DiscordSnowflake GuildId { get; init; } = null!;

        /// <summary>
        /// A set of guild members.
        /// </summary>
        [JsonPropertyName("members")]
        public DiscordGuildMember[] Members { get; init; } = null!;

        /// <summary>
        /// The chunk index in the expected chunks for this response (0 <= chunk_index < chunk_count).
        /// </summary>
        [JsonPropertyName("chunk_index")]
        public int ChunkIndex { get; init; }

        /// <summary>
        /// The total number of expected chunks for this response.
        /// </summary>
        [JsonPropertyName("chunk_count")]
        public int ChunkCount { get; init; }

        /// <summary>
        /// If passing an invalid id to REQUEST_GUILD_MEMBERS, it will be returned here.
        /// </summary>
        [JsonPropertyName("not_found")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Optional<DiscordSnowflake[]> NotFound { get; init; }

        /// <summary>
        /// If passing true to REQUEST_GUILD_MEMBERS, presences of the returned members will be here.
        /// </summary>
        [JsonPropertyName("presences")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Optional<DiscordUpdatePresencePayload> Presences { get; init; }

        /// <summary>
        /// The nonce used in the Guild Members Request.
        /// </summary>
        [JsonPropertyName("nonce")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Optional<string> Nonce { get; init; }
    }
}
