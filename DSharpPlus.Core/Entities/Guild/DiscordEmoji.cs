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
    /// Implements a <see href="https://discord.com/developers/docs/resources/emoji#emoji-object">Discord emoji</see>.
    /// </summary>
    public sealed record DiscordEmoji
    {
        /// <summary>
        /// The emoji's Id.
        /// </summary>
        [JsonPropertyName("id")]
        public DiscordSnowflake? Id { get; init; } = null!;

        /// <summary>
        /// The emoji's name.
        /// </summary>
        [JsonPropertyName("name")]
        public string? Name { get; internal set; } = null!;

        /// <summary>
        /// The roles allowed to use this emoji.
        /// </summary>
        [JsonPropertyName("roles")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Optional<DiscordSnowflake[]> Roles { get; internal set; }

        /// <summary>
        /// The user that created this emoji.
        /// </summary>
        [JsonPropertyName("user")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Optional<DiscordUser> User { get; internal set; } = null!;

        /// <summary>
        /// Whether this emoji must be wrapped in colons.
        /// </summary>
        [JsonPropertyName("require_colons")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Optional<bool> RequiresColons { get; internal set; }

        /// <summary>
        /// Whether this emoji is managed.
        /// </summary>
        [JsonPropertyName("managed")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Optional<bool> Managed { get; internal set; }

        /// <summary>
        /// Whether this emoji is animated.
        /// </summary>
        [JsonPropertyName("animated")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Optional<bool> Animated { get; internal set; }

        /// <summary>
        /// Whether this emoji can be used. May be false due to loss of Server Boosts.
        /// </summary>
        [JsonPropertyName("available")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Optional<bool> Available { get; internal set; }

        /// <exception cref="NullReferenceException">If the emoji does not have an id.</exception>
        public static implicit operator ulong(DiscordEmoji emoji) => emoji.Id!;

        /// <exception cref="NullReferenceException">If the emoji does not have an id.</exception>
        public static implicit operator DiscordSnowflake(DiscordEmoji emoji) => emoji.Id!;
    }
}
