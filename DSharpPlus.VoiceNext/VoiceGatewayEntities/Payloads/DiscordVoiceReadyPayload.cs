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
using Newtonsoft.Json;

namespace DSharpPlus.VoiceNext.VoiceGatewayEntities.Payloads
{
    /// <summary>
    /// The voice server should respond with an <see cref="VoiceNext.Enums.DiscordVoiceOpCode.Ready"/> payload, which informs us of the SSRC, UDP IP/port, and supported encryption modes the voice server expects.
    /// </summary>
    public sealed record DiscordVoiceReadyPayload
    {
        [JsonProperty("ssrc", NullValueHandling = NullValueHandling.Ignore)]
        public uint SSRC { get; internal set; }

        [JsonProperty("address", NullValueHandling = NullValueHandling.Ignore)]
        public string Address { get; internal set; } = null!;

        [JsonProperty("port", NullValueHandling = NullValueHandling.Ignore)]
        public ushort Port { get; internal set; }

        [JsonProperty("modes", NullValueHandling = NullValueHandling.Ignore)]
        public string[] Modes { get; internal set; } = null!;

        [Obsolete("HeartbeatInterval here is an erroneous field and should be ignored. The correct heartbeat_interval value comes from the Hello payload.")]
        [JsonProperty("heartbeat_interval", NullValueHandling = NullValueHandling.Ignore)]
        public int HeartbeatInterval { get; internal set; }
    }
}
