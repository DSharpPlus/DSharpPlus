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

using DSharpPlus.Core.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DSharpPlus.Core.Entities
{
    /// <remarks>
    /// See <see href="https://discord.com/developers/docs/topics/permissions#permissions">permissions</see> for more information about the allow and deny fields.
    /// </remarks>
    public sealed record DiscordChannelOverwrite
    {
        /// <summary>
        /// Role or user id.
        /// </summary>
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake Id { get; init; } = null!;

        /// <summary>
        /// Either 0 (role) or 1 (member).
        /// </summary>
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public int Type { get; init; }

        /// <summary>
        /// Permission bit set.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("allow", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordPermissions Allow { get; internal set; }

        /// <summary>
        /// Permission bit set.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("deny", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordPermissions Deny { get; internal set; }

        public static implicit operator ulong(DiscordChannelOverwrite channelOverwrite) => channelOverwrite.Id;
        public static implicit operator DiscordSnowflake(DiscordChannelOverwrite channelOverwrite) => channelOverwrite.Id;
    }
}
