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

using System.Collections.Generic;
using System.ComponentModel;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink.Entities.Filters;
using Newtonsoft.Json;

namespace DSharpPlus.Lavalink
{
    public class LavalinkVoiceState
    {
        /// <summary>
        /// The Discord voice token to authenticate with
        /// </summary>
        [JsonProperty("token", NullValueHandling = NullValueHandling.Ignore)]
        public string Token { get; set; }
        /// <summary>
        /// The Discord voice endpoint to connect to
        /// </summary>
        [JsonProperty("endpoint", NullValueHandling = NullValueHandling.Ignore)]
        public string Endpoint { get; set; }
        /// <summary>
        /// The Discord voice session id to authenticate with
        /// </summary>
        [JsonProperty("sessionId", NullValueHandling = NullValueHandling.Ignore)]
        public string SessionId { get; set; }
        /// <summary>
        /// Whether the player is connected. Response only
        /// </summary>
        [JsonProperty("connected", NullValueHandling = NullValueHandling.Ignore)]
        [JsonIgnore]
        public bool? IsConnected { get; internal set; }
        /// <summary>
        /// Roundtrip latency in milliseconds to the voice gateway (-1 if not connected). Response only
        /// </summary>
        [JsonIgnore]
        [JsonProperty("ping", NullValueHandling = NullValueHandling.Ignore)]
        public int? Ping { get; internal set; }
    }


    public class LavalinkFilters
    {
        [JsonProperty("volume", NullValueHandling = NullValueHandling.Ignore)]
        public float? Volume { get; internal set; }
        [JsonProperty("equalizers", NullValueHandling = NullValueHandling.Ignore)]
        public LavalinkBandAdjustment[] Equalizers { get; internal set; }
        [JsonProperty("karaoke", NullValueHandling = NullValueHandling.Ignore)]
        public LavalinkKaraokeFilter? Karaoke { get; internal set; }
        [JsonProperty("timescale", NullValueHandling = NullValueHandling.Ignore)]
        public LavalinkTimescaleFilter? Timescale { get; internal set; }
        [JsonProperty("tremolo", NullValueHandling = NullValueHandling.Ignore)]
        public LavalinkTremoloFilter? Tremolo { get; internal set; }
        [JsonProperty("vibrato", NullValueHandling = NullValueHandling.Ignore)]
        public LavalinkVibratoFilter? Vibrato { get; internal set; }
        [JsonProperty("rotation", NullValueHandling = NullValueHandling.Ignore)]
        public LavalinkRotationFilter? Rotation { get; internal set; }
        [JsonProperty("distortion", NullValueHandling = NullValueHandling.Ignore)]
        public LavalinkDistortionFilter? Distortion { get; internal set; }
        [JsonProperty("channelMix", NullValueHandling = NullValueHandling.Ignore)]
        public LavalinkChannelMixFilter? ChannelMix { get; internal set; }
        [JsonProperty("lowPass", NullValueHandling = NullValueHandling.Ignore)]
        public LavalinkLowPassFilter? LowPass { get; internal set; }
    }

    public class LavalinkPlayerUpdatePayload
    {
        /// <summary>
        /// The encoded track base64 to play. null stops the current track
        /// </summary>
        [JsonProperty("encodedTrack", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [DefaultValue("")]
        public string? EncodedTrack { get; set; } = "";
        /// <summary>
        /// The track identifier to play
        /// </summary>
        [JsonProperty("identifier", NullValueHandling = NullValueHandling.Ignore)]
        public string? Identifier { get; set; }
        /// <summary>
        /// The track position in milliseconds
        /// </summary>
        [JsonProperty("position", NullValueHandling = NullValueHandling.Ignore)]
        public long? Position { get; set; }
        /// <summary>
        /// The track end time in milliseconds
        /// </summary>
        [JsonProperty("endTime", NullValueHandling = NullValueHandling.Ignore)]
        public long? EndTime { get; set; }
        /// <summary>
        /// The player volume from 0 to 1000
        /// </summary>
        [JsonProperty("volume", NullValueHandling = NullValueHandling.Ignore)]
        public int? Volume { get; set; }
        /// <summary>
        /// Whether the player is paused
        /// </summary>
        [JsonProperty("paused", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Paused { get; set; }
        /// <summary>
        /// The new filters to apply. This will override all previously applied filters
        /// </summary>
        [JsonProperty("filters", NullValueHandling = NullValueHandling.Ignore)]
        public LavalinkFilters? Filters { get; set; }
        /// <summary>
        /// Information required for connecting to Discord, without connected or ping
        /// </summary>
        [JsonProperty("voice", NullValueHandling = NullValueHandling.Ignore)]
        public LavalinkVoiceState? VoiceState { get; set; }
    }
    public sealed class LavalinkPlayer
    {
        /// <summary>
        /// The guild id of the player
        /// </summary>
        [JsonProperty("guildId")]
        public long GuildId { get; internal set; }
        /// <summary>
        /// The current playing track
        /// </summary>
        [JsonProperty("track")]
        public LavalinkTrack? Track { get; internal set; }
        /// <summary>
        /// The volume of the player, range 0-1000, in percentage
        /// </summary>
        [JsonProperty("volume")]
        public int Volume { get; internal set; }
        /// <summary>
        /// Whether the player is paused
        /// </summary>
        [JsonProperty("paused")]
        public bool IsPaused { get; internal set; }
        /// <summary>
        /// The voice state of the player
        /// </summary>
        [JsonProperty("voice")]
        public LavalinkVoiceState VoiceState { get; internal set; }
        /// <summary>
        /// The filters used by the player
        /// </summary>
        [JsonProperty("filters")]
        public LavalinkFilters Filters { get; internal set; }

    }
}
