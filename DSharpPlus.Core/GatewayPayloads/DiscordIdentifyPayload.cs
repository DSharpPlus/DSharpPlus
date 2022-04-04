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
using DSharpPlus.Core.Entities;
using DSharpPlus.Core.Enums;
using Newtonsoft.Json;

namespace DSharpPlus.Core.GatewayPayloads
{
    /// <summary>
    /// Used to trigger the initial handshake with the gateway.
    /// </summary>
    public sealed record DiscordIdentifyPayload
    {
        /// <summary>
        /// The authentication token.
        /// </summary>
        [JsonProperty("token", NullValueHandling = NullValueHandling.Ignore)]
        public string Token { get; init; } = null!;

        /// <summary>
        /// The connection properties.
        /// </summary>
        [JsonProperty("properties", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordIdentifyConnectionProperties Properties { get; init; } = null!;

        /// <summary>
        /// Whether this connection supports compression of packets.
        /// </summary>
        [JsonProperty("compress", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<bool> Compress { get; init; } = true;

        /// <summary>
        /// A value between 50 and 250, total number of members where the gateway will stop sending offline members in the guild member list.
        /// </summary>
        [JsonProperty("large_threshold", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<int> LargeThreshold { get; init; } = 50;

        /// <summary>
        /// Used for Guild Sharding.
        /// </summary>
        [JsonProperty("shard", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<Dictionary<int, int>> Shard { get; init; }

        /// <summary>
        /// The presence structure for initial presence information.
        /// </summary>
        [JsonProperty("presence", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordUpdatePresencePayload> Presence { get; init; }

        /// <summary>
        /// The <see cref="DiscordGatewayIntents"/> you wish to receive.
        /// </summary>
        [JsonProperty("intents", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordGatewayIntents Intents { get; init; }
    }
}
