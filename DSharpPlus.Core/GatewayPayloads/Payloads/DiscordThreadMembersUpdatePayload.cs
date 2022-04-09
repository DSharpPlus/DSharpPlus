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
    /// Sent when anyone is added to or removed from a thread. If the current user does not have the <see cref="Enums.DiscordGatewayIntents.GuildMembers"/>, then this event will only be sent if the current user was added to or removed from the thread.
    /// </summary>
    /// <remarks>
    /// In this gateway event, the thread member objects will also include the <see cref="DiscordGuildMember"/> and nullable <see cref="DiscordUpdatePresencePayload"/> for each added thread member.
    /// </remarks>
    public sealed record DiscordThreadMembersUpdatePayload
    {
        /// <summary>
        /// The id of the thread.
        /// </summary>
        [JsonPropertyName("id")]
        public DiscordSnowflake Id { get; init; } = null!;

        /// <summary>
        /// The id of the guild.
        /// </summary>
        [JsonPropertyName("guild_id")]
        public DiscordSnowflake GuildId { get; init; } = null!;

        /// <summary>
        /// The approximate number of members in the thread, capped at 50.
        /// </summary>
        [JsonPropertyName("member_count")]
        public int MemberCount { get; init; }

        /// <summary>
        /// The users who were added to the thread.
        /// </summary>
        [JsonPropertyName("added_members")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Optional<DiscordThreadMember[]> AddedMembers { get; init; } = null!;

        /// <summary>
        /// The id of the users who were removed from the thread.
        /// </summary>
        [JsonPropertyName("removed_member_ids")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Optional<DiscordSnowflake[]> RemovedMemberIds { get; init; } = null!;
    }
}
