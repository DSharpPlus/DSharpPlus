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
using System.Drawing;
using System.Text.Json.Serialization;
using DSharpPlus.Core.Enums;

namespace DSharpPlus.Core.Entities
{
    /// <summary>
    /// Roles represent a set of permissions attached to a group of users. Roles have names, colors, and can be "pinned" to the side bar, causing their members to be listed separately. Roles can have separate permission profiles for the global context (guild) and channel context. The <c>@everyone</c> role has the same ID as the guild it belongs to.
    /// </summary>
    public sealed record DiscordRole
    {
        /// <summary>
        /// Role Id.
        /// </summary>
        [JsonPropertyName("id")]
        public DiscordSnowflake Id { get; init; } = null!;

        /// <summary>
        /// Name of the role.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; private set; } = null!;

        /// <summary>
        /// The color of the role.
        /// </summary>
        [JsonPropertyName("color")]
        public Color Color { get; private set; }

        /// <summary>
        /// If this role is pinned in the user listing.
        /// </summary>
        [JsonPropertyName("hoist")]
        public bool IsHoisted { get; private set; }

        /// <summary>
        /// The role icon hash.
        /// </summary>
        [JsonPropertyName("icon")]
        public Optional<string?> Icon { get; private set; }

        /// <summary>
        /// The role unicode emoji.
        /// </summary>
        [JsonPropertyName("unicode_emoji")]
        public Optional<string?> UnicodeEmoji { get; private set; }

        /// <summary>
        /// The position of this role.
        /// </summary>
        [JsonPropertyName("position")]
        public int Position { get; private set; }

        /// <summary>
        /// The Discord permissions of this role.
        /// </summary>
        [JsonPropertyName("permissions")]
        public DiscordPermissions Permissions { get; private set; }

        /// <summary>
        /// Whether this role is managed by an integration.
        /// </summary>
        [JsonPropertyName("managed")]
        public bool IsManaged { get; init; }

        /// <summary>
        /// Whether this role is mentionable.
        /// </summary>
        [JsonPropertyName("mentionable")]
        public bool IsMentionable { get; private set; }

        /// <summary>
        /// The tags this role has.
        /// </summary>
        [JsonPropertyName("tags")]
        public Optional<DiscordRoleTags> Tags { get; init; }

        internal DiscordRole() { }

        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(Id);
            hash.Add(Name);
            hash.Add(Color);
            hash.Add(IsHoisted);
            hash.Add(Icon);
            hash.Add(UnicodeEmoji);
            hash.Add(Position);
            hash.Add(Permissions);
            hash.Add(IsManaged);
            hash.Add(IsMentionable);
            hash.Add(Tags);
            return hash.ToHashCode();
        }
    }
}
