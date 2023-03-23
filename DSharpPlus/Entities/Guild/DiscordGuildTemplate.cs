// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2023 DSharpPlus Contributors
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
using Newtonsoft.Json;

namespace DSharpPlus.Entities
{
    public class DiscordGuildTemplate
    {
        /// <summary>
        /// Gets the template code.
        /// </summary>
        [JsonProperty("code", NullValueHandling = NullValueHandling.Ignore)]
        public string Code { get; internal set; }

        /// <summary>
        /// Gets the name of the template.
        /// </summary>
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; internal set; }

        /// <summary>
        /// Gets the description of the template.
        /// </summary>
        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; internal set; }

        /// <summary>
        /// Gets the number of times the template has been used.
        /// </summary>
        [JsonProperty("usage_count", NullValueHandling = NullValueHandling.Ignore)]
        public int UsageCount { get; internal set; }

        /// <summary>
        /// Gets the ID of the creator of the template.
        /// </summary>
        [JsonProperty("creator_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong CreatorId { get; internal set; }

        /// <summary>
        /// Gets the creator of the template.
        /// </summary>
        [JsonProperty("creator", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordUser Creator { get; internal set; }

        /// <summary>
        /// Date the template was created.
        /// </summary>
        [JsonProperty("created_at", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset CreatedAt { get; internal set; }

        /// <summary>
        /// Date the template was updated.
        /// </summary>
        [JsonProperty("updated_at", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset UpdatedAt { get; internal set; }

        /// <summary>
        /// Gets the ID of the source guild.
        /// </summary>
        [JsonProperty("source_guild_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong SourceGuildId { get; internal set; }

        /// <summary>
        /// Gets the source guild.
        /// </summary>
        [JsonProperty("serialized_source_guild", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordGuild SourceGuild { get; internal set; }

        /// <summary>
        /// Gets whether the template has not synced changes.
        /// </summary>
        [JsonProperty("is_dirty", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsDirty { get; internal set; }
    }
}
