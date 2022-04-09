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

namespace DSharpPlus.Core.Entities
{
    public sealed record DiscordAuditLogEntryInfo
    {
        /// <summary>
        /// The channel in which the entities were targeted.
        /// </summary>
        [JsonPropertyName("channel_id")]
        public DiscordSnowflake ChannelId { get; init; } = null!;

        /// <summary>
        /// The number of entities that were targeted.
        /// </summary>
        [JsonPropertyName("count")]
        public string Count { get; init; } = null!;

        /// <summary>
        /// The number of days after which inactive members were kicked.
        /// </summary>
        [JsonPropertyName("delete_member_days")]
        public string DeleteMemberDays { get; init; } = null!;

        /// <summary>
        /// The id of the overwritten entity.
        /// </summary>
        [JsonPropertyName("id")]
        public DiscordSnowflake Id { get; init; } = null!;

        /// <summary>
        /// The number of members removed by the prune.
        /// </summary>
        [JsonPropertyName("members_removed")]
        public string MembersRemoved { get; init; } = null!;

        /// <summary>
        /// The id of the message that was targeted.
        /// </summary>
        [JsonPropertyName("message_id")]
        public DiscordSnowflake MessageId { get; init; } = null!;

        /// <summary>
        /// The name of the role if <see cref="Type"/> is "0" (not present if <see cref="Type"/> is "1").
        /// </summary>
        [JsonPropertyName("role_name")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Optional<string> RoleName { get; init; } = null!;

        /// <summary>
        /// The type of overwritten entity - "0" for "role" or "1" for "member".
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; init; } = null!;

        public static implicit operator ulong(DiscordAuditLogEntryInfo auditLogEntryInfo) => auditLogEntryInfo.Id;
        public static implicit operator DiscordSnowflake(DiscordAuditLogEntryInfo auditLogEntryInfo) => auditLogEntryInfo.Id;
    }
}
