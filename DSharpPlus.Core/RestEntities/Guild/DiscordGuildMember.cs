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
using DSharpPlus.Core.Attributes;
using DSharpPlus.Core.Enums;
using Newtonsoft.Json;

namespace DSharpPlus.Core.Entities
{
    /// <summary>
    /// Represents a member of a guild. Implements a <see href="https://discord.com/developers/docs/resources/guild#guild-member-object">Discord Guild Member</see>.
    /// </summary>
    [DiscordGatewayEventName("GUILD_MEMBER_ADD")]
    public sealed record DiscordGuildMember
    {
        /// <summary>
        /// The user this guild member represents.
        /// </summary>
        /// <remarks>
        /// The <c>user</c> object won't be included in the member object attached to <c>MESSAGE_CREATE</c> and <c>MESSAGE_UPDATE</c> gateway events.
        /// </remarks>
        [JsonProperty("user", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordUser> User { get; init; }

        /// <summary>
        /// This user's guild nickname.
        /// </summary>
        [JsonProperty("nick", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<string?> Nick { get; init; }

        /// <summary>
        /// The member's guild avatar hash.
        /// </summary>
        [JsonProperty("avatar", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<string?> Avatar { get; init; }

        /// <summary>
        /// Array of <see href="https://discord.com/developers/docs/topics/permissions#role-object">role</see> object ids.
        /// </summary>
        [JsonProperty("roles", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake[] Roles { get; init; } = Array.Empty<DiscordSnowflake>();

        /// <summary>
        /// When the user joined the guild.
        /// </summary>
        /// <remarks>
        /// Resets when the member leaves and rejoins the guild.
        /// </remarks>
        [JsonProperty("joined_at", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset JoinedAt { get; init; }

        /// <summary>
        /// When the user started <see href="https://support.discord.com/hc/en-us/articles/360028038352-Server-Boosting-">boosting</see> the guild.
        /// </summary>
        /// <remarks>
        /// Can also be seen as "Nitro boosting since".
        /// </remarks>
        [JsonProperty("premium_since", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DateTimeOffset?> PremiumSince { get; init; }

        /// <summary>
        /// Whether the user is deafened in voice channels.
        /// </summary>
        /// <remarks>
        /// This could be a self deafen, or a server deafen.
        /// </remarks>
        [JsonProperty("deaf", NullValueHandling = NullValueHandling.Ignore)]
        public bool Deaf { get; init; }

        /// <summary>
        /// Whether the user is muted in voice channels.
        /// </summary>
        /// <remarks>
        /// This could be a self mute, or a server mute.
        /// </remarks>
        [JsonProperty("mute", NullValueHandling = NullValueHandling.Ignore)]
        public bool Mute { get; init; }

        /// <summary>
        /// Whether the user has not yet passed the guild's <see href="https://discord.com/developers/docs/resources/guild#membership-screening-object">Membership Screening</see> requirements.
        /// </summary>
        [JsonProperty("pending", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<bool> Pending { get; init; }

        /// <summary>
        /// Total permissions of the member in the channel, including overwrites, returned when in the interaction object.
        /// </summary>
        /// <remarks>
        /// This is only available on an interaction, such as a Slash Command.
        /// </remarks>
        [JsonProperty("permissions", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordPermissions> Permissions { get; init; }

        /// <summary>
        /// When the user's <see href="https://support.discord.com/hc/en-us/articles/4413305239191-Time-Out-FAQ">timeout</see> will expire and the user will be able to communicate in the guild again, null or a time in the past if the user is not timed out.
        /// </summary>
        /// <remarks>
        /// Could also be seen as "muted until".
        /// </remarks>
        [JsonProperty("communication_disabled_until", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DateTimeOffset?> CommunicationDisabledUntil { get; init; }

        /// <summary>
        /// The id of the guild.
        /// </summary>
        /// <remarks>
        /// Only sent in the GUILD_MEMBER_ADD and GUILD_MEMBER_UPDATE payloads.
        /// </remarks>
        public Optional<DiscordSnowflake> GuildId { get; init; }
    }
}
