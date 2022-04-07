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
    /// Represents a code that when used, creates a guild based on a snapshot of an existing guild.
    /// </summary>
    public sealed record DiscordGuildTemplate
    {
        /// <summary>
        /// The template code (unique ID).
        /// </summary>
        [JsonPropertyName("code")]
        public string Code { get; init; } = null!;

        /// <summary>
        /// The template name.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; init; } = null!;

        /// <summary>
        /// The description for the template.
        /// </summary>
        [JsonPropertyName("description")]
        public string? Description { get; internal set; }

        /// <summary>
        /// The number of times this template has been used.
        /// </summary>
        [JsonPropertyName("usage_count")]
        public int UsageCount { get; internal set; }

        /// <summary>
        /// The ID of the user who created the template.
        /// </summary>
        [JsonPropertyName("creator_id")]
        public DiscordSnowflake CreatorId { get; init; } = null!;

        /// <summary>
        /// The user who created the template.
        /// </summary>
        [JsonPropertyName("creator")]
        public DiscordUser Creator { get; init; } = null!;

        /// <summary>
        /// When this template was created.
        /// </summary>
        [JsonPropertyName("created_at")]
        public DateTimeOffset CreatedAt { get; init; }

        /// <summary>
        /// When this template was last synced to the source guild.
        /// </summary>
        [JsonPropertyName("updated_at")]
        public DateTimeOffset UpdatedAt { get; internal set; }

        /// <summary>
        /// The ID of the guild this template is based on.
        /// </summary>
        [JsonPropertyName("source_guild_id")]
        public DiscordSnowflake SourceGuildId { get; init; } = null!;

        /// <summary>
        /// The guild snapshot this template contains.
        /// </summary>
        [JsonPropertyName("serialized_source_guild")]
        public DiscordGuild SerializedSourceGuild { get; internal set; } = null!;

        /// <summary>
        /// Whether the template has unsynced changes.
        /// </summary>
        [JsonPropertyName("is_dirty")]
        public bool IsDirty { get; internal set; }
    }
}
