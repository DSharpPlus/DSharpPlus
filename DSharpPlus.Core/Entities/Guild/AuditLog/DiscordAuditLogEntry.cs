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
using DSharpPlus.Core.Enums;

namespace DSharpPlus.Core.Entities
{
    public sealed record DiscordAuditLogEntry
    {
        /// <summary>
        /// The id of the affected entity (webhook, user, role, etc.)
        /// </summary>
        [JsonPropertyName("id")]
        public string? TargetId { get; init; }

        /// <summary>
        /// The changes made to the <see cref="TargetId"/>
        /// </summary>
        [JsonPropertyName("changes")]
        public Optional<DiscordAuditLogChange[]> Changes { get; init; }

        /// <summary>
        /// The user who made the changes.
        /// </summary>
        [JsonPropertyName("user_id")]
        public DiscordSnowflake? UserId { get; init; }

        /// <summary>
        /// The id of the entry.
        /// </summary>
        [JsonPropertyName("id")]
        public DiscordSnowflake Id { get; init; } = null!;

        /// <summary>
        /// The type of action that occurred.
        /// </summary>
        [JsonPropertyName("action_type")]
        public DiscordAuditLogEvent ActionType { get; init; }

        /// <summary>
        /// Additional info for certain action types.
        /// </summary>
        [JsonPropertyName("options")]
        public Optional<DiscordAuditLogEntryInfo> Options { get; init; }

        /// <summary>
        /// The reason for the change (0-512 characters)
        /// </summary>
        [JsonPropertyName("reason")]
        public Optional<string> Reason { get; init; }

        public static implicit operator ulong(DiscordAuditLogEntry auditLogEntry) => auditLogEntry.Id;
        public static implicit operator DiscordSnowflake(DiscordAuditLogEntry auditLogEntry) => auditLogEntry.Id;
    }
}
