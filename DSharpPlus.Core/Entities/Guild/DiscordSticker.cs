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
using DSharpPlus.Core.Enums;

namespace DSharpPlus.Core.Entities
{
    /// <summary>
    /// Represents a sticker that can be sent in messages.
    /// </summary>
    public sealed record DiscordSticker
    {
        /// <summary>
        /// The id of the sticker.
        /// </summary>
        public DiscordSnowflake Id { get; init; } = null!;

        /// <summary>
        /// For standard stickers, id of the pack the sticker is from.
        /// </summary>
        public Optional<DiscordSnowflake> PackId { get; init; }

        /// <summary>
        /// The name of the sticker.
        /// </summary>
        public string Name { get; internal set; } = null!;

        /// <summary>
        /// The description of the sticker.
        /// </summary>
        public string? Description { get; internal set; }

        /// <summary>
        /// Autocomplete/suggestion tags for the sticker (max 200 characters).
        /// </summary>
        public string Tags { get; internal set; } = null!;

        /// <summary>
        /// Deprecated previously the sticker asset hash, now an empty string
        /// </summary>
        [Obsolete("Deprecated previously the sticker asset hash, now an empty string")]
        public Optional<string> Asset { get; set; } = null!;

        /// <summary>
        /// The type of sticker.
        /// </summary>
        public DiscordStickerType Type { get; init; }

        /// <summary>
        /// The type of sticker format.
        /// </summary>
        public DiscordStickerFormatType FormatType { get; init; }

        /// <summary>
        /// Whether this guild sticker can be used, may be false due to loss of Server Boosts.
        /// </summary>
        public Optional<bool> Available { get; init; }

        /// <summary>
        /// The id of the guild that owns this sticker.
        /// </summary>
        public Optional<DiscordSnowflake?> GuildId { get; init; }

        /// <summary>
        /// The user that uploaded the guild sticker.
        /// </summary>
        public Optional<DiscordUser> User { get; init; }

        /// <summary>
        /// The standard sticker's sort order within its pack.
        /// </summary>
        public Optional<int> SortValue { get; init; }
    }
}
