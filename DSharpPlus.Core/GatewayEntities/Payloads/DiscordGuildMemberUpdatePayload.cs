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
using DSharpPlus.Core.Attributes;
using DSharpPlus.Core.RestEntities;
using Newtonsoft.Json;

namespace DSharpPlus.Core.GatewayEntities.Payloads
{
    /// <summary>
    /// Sent when a guild member is updated. This will also fire when the user object of a guild member changes.
    /// </summary>
    [DiscordGatewayPayload("GUILD_MEMBER_UPDATE")]
    public sealed record DiscordGuildMemberUpdatePayload
    {
        /// <summary>
        /// The id of the guild.
        /// </summary>
        [JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake GuildId { get; init; } = null!;

        /// <summary>
        /// User's role ids.
        /// </summary>
        [JsonProperty("roles", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<DiscordSnowflake> Roles { get; init; } = Array.Empty<DiscordSnowflake>();

        /// <summary>
        /// The user.
        /// </summary>
        [JsonProperty("user", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordUser User { get; init; } = null!;

        /// <summary>
        /// The nickname of the user in the guild.
        /// </summary>
        [JsonProperty("nick", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<string?> Nick { get; init; }

        /// <summary>
        /// The member's guild avatar hash.
        /// </summary>
        [JsonProperty("avatar", NullValueHandling = NullValueHandling.Ignore)]
        public string? Avatar { get; init; }

        /// <summary>
        /// When the user joined the guild.
        /// </summary>
        [JsonProperty("joined_at", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? JoinedAt { get; init; }

        /// <summary>
        /// When the user started boosting the guild.
        /// </summary>
        [JsonProperty("premium_since", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DateTimeOffset?> PremiumSince { get; init; }

        /// <summary>
        /// Whether the user is deafened in voice channels.
        /// </summary>
        [JsonProperty("deaf", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<bool> Deaf { get; init; }

        /// <summary>
        /// Whether the user is muted in voice channels.
        /// </summary>
        [JsonProperty("mute", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<bool> Mute { get; init; }

        /// <summary>
        /// Whether the user has not yet passed the guild's Membership Screening requirements.
        /// </summary>
        [JsonProperty("pending", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<bool> Pending { get; init; }

        /// <summary>
        /// When the user's timeout will expire and the user will be able to communicate in the guild again, null or a time in the past if the user is not timed out.
        /// </summary>
        [JsonProperty("communication_disabled_until", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DateTimeOffset?> CommunicationDisabledUntil { get; init; }
    }
}
