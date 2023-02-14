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

using System.Collections.Generic;
using Newtonsoft.Json;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents a collection of Discord snowflake objects resolved from interaction arguments.
    /// </summary>
    public sealed class DiscordInteractionResolvedCollection
    {
        /// <summary>
        /// Gets the resolved user objects, if any.
        /// </summary>
        [JsonProperty("users", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyDictionary<ulong, DiscordUser> Users { get; internal set; }

        /// <summary>
        /// Gets the resolved member objects, if any.
        /// </summary>
        [JsonProperty("members", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyDictionary<ulong, DiscordMember> Members { get; internal set; }

        /// <summary>
        /// Gets the resolved channel objects, if any.
        /// </summary>
        [JsonProperty("channels", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyDictionary<ulong, DiscordChannel> Channels { get; internal set; }

        /// <summary>
        /// Gets the resolved role objects, if any.
        /// </summary>
        [JsonProperty("roles", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyDictionary<ulong, DiscordRole> Roles { get; internal set; }

        /// <summary>
        /// Gets the resolved message objects, if any.
        /// </summary>
        [JsonProperty("messages", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyDictionary<ulong, DiscordMessage> Messages { get; internal set; }

        /// <summary>
        /// The resolved attachment objects, if any.
        /// </summary>
        [JsonProperty("attachments")]
        public IReadOnlyDictionary<ulong, DiscordAttachment> Attachments { get; internal set; }
    }
}
