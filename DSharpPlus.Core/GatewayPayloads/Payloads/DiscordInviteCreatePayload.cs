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
using DSharpPlus.Core.Entities;
using DSharpPlus.Core.Enums;

namespace DSharpPlus.Core.Gateway.Payloads
{
    /// <summary>
    /// Sent when a new invite to a channel is created.
    /// </summary>
    public sealed record DiscordInviteCreatePayload
    {
        /// <summary>
        /// The channel the invite is for.
        /// </summary>
        [JsonPropertyName("channel_id")]
        public DiscordSnowflake ChannelId { get; init; } = null!;

        /// <summary>
        /// The unique invite code.
        /// </summary>
        [JsonPropertyName("code")]
        public string Code { get; init; } = null!;

        /// <summary>
        /// The time at which the invite was created.
        /// </summary>
        [JsonPropertyName("created_at")]
        public DateTimeOffset CreatedAt { get; init; }

        /// <summary>
        /// The guild of the invite.
        /// </summary>
        [JsonPropertyName("guild_id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Optional<DiscordSnowflake> GuildId { get; init; }

        /// <summary>
        /// The user that created the invite.
        /// </summary>
        [JsonPropertyName("inviter")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Optional<DiscordUser> Inviter { get; init; }

        /// <summary>
        /// How long the invite is valid for (in seconds).
        /// </summary>
        [JsonPropertyName("max_age")]
        public int MaxAge { get; init; }

        /// <summary>
        /// The maximum number of times the invite can be used.
        /// </summary>
        [JsonPropertyName("max_uses")]
        public int MaxUses { get; init; }

        /// <summary>
        /// The type of target for this voice channel invite.
        /// </summary>
        [JsonPropertyName("target_type")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Optional<DiscordGuildInviteTargetType> TargetType { get; init; }

        /// <summary>
        /// The user whose stream to display for this voice channel stream invite.
        /// </summary>
        [JsonPropertyName("target_user")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Optional<DiscordUser> TargetUser { get; init; }

        /// <summary>
        /// The embedded application to open for this voice channel embedded application invite.
        /// </summary>
        [JsonPropertyName("target_application")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Optional<DiscordApplication> TargetApplication { get; init; }

        /// <summary>
        /// Whether or not the invite is temporary (invited users will be kicked on disconnect unless they're assigned a role).
        /// </summary>
        [JsonPropertyName("temporary")]
        public bool Temporary { get; init; }

        /// <summary>
        /// How many times the invite has been used (always will be 0).
        /// </summary>
        [JsonPropertyName("uses")]
        public int Uses { get; init; }
    }
}
