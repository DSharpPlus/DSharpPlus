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
using DSharpPlus.Entities;
using Newtonsoft.Json;

namespace DSharpPlus.Core.VoiceGatewayEntities
{
    /// <summary>
    /// To inform the gateway of our intent to establish voice connectivity, we first send an Voice State Update payload.
    /// </summary>
    public sealed record DiscordVoiceStateUpdate
    {
        /// <summary>
        /// The guild id this voice state is for.
        /// </summary>
        [JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<ulong> GuildId { get; internal set; }

        /// <summary>
        /// The channel id this user is connected to.
        /// </summary>
        [JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong? ChannelId { get; internal set; }

        /// <summary>
        /// The user id this voice state is for.
        /// </summary>
        [JsonProperty("user_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong UserId { get; internal set; }

        /// <summary>
        /// The guild member this voice state is for.
        /// </summary>
        [JsonProperty("member", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordMember> Member { get; internal set; }

        /// <summary>
        /// The session id for this voice state.
        /// </summary>
        [JsonProperty("session_id", NullValueHandling = NullValueHandling.Ignore)]
        public string SessionId { get; internal set; } = null!;

        /// <summary>
        /// Whether this user is locally muted.
        /// </summary>
        [JsonProperty("self_mute", NullValueHandling = NullValueHandling.Ignore)]
        public bool SelfMute { get; internal set; }

        /// <summary>
        /// Whether this user is locally deafened.
        /// </summary>
        [JsonProperty("self_deaf", NullValueHandling = NullValueHandling.Ignore)]
        public bool SelfDeaf { get; internal set; }

        /// <summary>
        /// Whether this user is muted by the server.
        /// </summary>
        [JsonProperty("mute", NullValueHandling = NullValueHandling.Ignore)]
        public bool Mute { get; internal set; }

        /// <summary>
        /// Whether this user is deafened by the server
        /// </summary>
        [JsonProperty("deaf", NullValueHandling = NullValueHandling.Ignore)]
        public bool Deaf { get; internal set; }

        /// <summary>
        /// Whether this user is streaming using "Go Live".
        /// </summary>
        [JsonProperty("self_stream", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<bool> SelfStream { get; internal set; }

        /// <summary>
        /// Whether this user's camera is enabled.
        /// </summary>
        [JsonProperty("self_video", NullValueHandling = NullValueHandling.Ignore)]
        public bool SelfVideo { get; internal set; }

        /// <summary>
        /// Whether this user is muted by the current user.
        /// </summary>
        [JsonProperty("suppress", NullValueHandling = NullValueHandling.Ignore)]
        public bool Supress { get; internal set; }

        /// <summary>
        /// The time at which the user requested to speak.
        /// </summary>
        [JsonProperty("request_to_speak_timestamp", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? RequestToSpeakTimestamp { get; internal set; }
    }
}
