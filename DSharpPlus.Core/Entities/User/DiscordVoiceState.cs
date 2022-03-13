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
    public sealed record DiscordVoiceState
    {
        /// <summary>
        /// The guild id this voice state is for.
        /// </summary>
        [JsonPropertyName("guild_id")]
        public Optional<DiscordSnowflake> GuildId { get; init; }

        /// <summary>
        /// The channel id this user is connected to.
        /// </summary>
        [JsonPropertyName("channel_id")]
        public Optional<DiscordSnowflake> ChannelId { get; init; }

        /// <summary>
        /// The user id this voice state is for.
        /// </summary>
        [JsonPropertyName("user_id")]
        public DiscordSnowflake UserId { get; init; } = null!;

        /// <summary>
        /// The guild member this voice state is for.
        /// </summary>
        [JsonPropertyName("member")]
        public Optional<DiscordGuildMember> Member { get; init; }

        /// <summary>
        /// The session id for this voice state.
        /// </summary>
        [JsonPropertyName("session_id")]
        public string SessionId { get; init; } = null!;

        /// <summary>
        /// Whether this user is locally muted.
        /// </summary>
        [JsonPropertyName("self_mute")]
        public bool IsSelfMuted { get; private set; }

        /// <summary>
        /// Whether this user is locally deafened.
        /// </summary>
        [JsonPropertyName("self_deaf")]
        public bool IsSelfDeafened { get; private set; }

        /// <summary>
        /// Whether this user is muted by the server.
        /// </summary>
        [JsonPropertyName("mute")]
        public bool IsServerMuted { get; private set; }

        /// <summary>
        /// Whether this user is deafened by the server
        /// </summary>
        [JsonPropertyName("deaf")]
        public bool IsServerDeafened { get; private set; }

        /// <summary>
        /// Whether this user is streaming using "Go Live".
        /// </summary>
        [JsonPropertyName("self_stream")]
        public Optional<bool> SelfStream { get; private set; }

        /// <summary>
        /// Whether this user's camera is enabled.
        /// </summary>
        [JsonPropertyName("self_video")]
        public bool SelfVideo { get; private set; }

        /// <summary>
        /// Whether this user is muted by the current user.
        /// </summary>
        [JsonPropertyName("suppress")]
        public bool IsSuppressed { get; private set; }

        /// <summary>
        /// The time at which the user requested to speak.
        /// </summary>
        [JsonPropertyName("request_to_speak_timestamp")]
        public Optional<DateTimeOffset> RequestedToSpeakAt { get; private set; }
    }
}
