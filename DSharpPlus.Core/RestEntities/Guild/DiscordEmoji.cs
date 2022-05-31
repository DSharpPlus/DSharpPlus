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
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DSharpPlus.Core.RestEntities
{
    /// <summary>
    /// Implements a <see href="https://discord.com/developers/docs/resources/emoji#emoji-object">Discord emoji</see>.
    /// </summary>
    public sealed record DiscordEmoji
    {
        /// <summary>
        /// The emoji's Id.
        /// </summary>
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake? Id { get; init; } = null!;

        /// <summary>
        /// The emoji's name.
        /// </summary>
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string? Name { get; init; } = null!;

        /// <summary>
        /// The roles allowed to use this emoji.
        /// </summary>
        [JsonProperty("roles", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<IReadOnlyList<DiscordSnowflake>> Roles { get; init; }

        /// <summary>
        /// The user that created this emoji.
        /// </summary>
        [JsonProperty("user", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordUser> User { get; init; } = null!;

        /// <summary>
        /// Whether this emoji must be wrapped in colons.
        /// </summary>
        [JsonProperty("require_colons", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<bool> RequiresColons { get; init; }

        /// <summary>
        /// Whether this emoji is managed.
        /// </summary>
        [JsonProperty("managed", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<bool> Managed { get; init; }

        /// <summary>
        /// Whether this emoji is animated.
        /// </summary>
        [JsonProperty("animated", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<bool> Animated { get; init; }

        /// <summary>
        /// Whether this emoji can be used. May be false due to loss of Server Boosts.
        /// </summary>
        [JsonProperty("available", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<bool> Available { get; init; }

        /// <exception cref="NullReferenceException">If the emoji does not have an id.</exception>
        public static implicit operator ulong(DiscordEmoji emoji) => emoji.Id!;

        /// <exception cref="NullReferenceException">If the emoji does not have an id.</exception>
        public static implicit operator DiscordSnowflake(DiscordEmoji emoji) => emoji.Id!;
    }
}
