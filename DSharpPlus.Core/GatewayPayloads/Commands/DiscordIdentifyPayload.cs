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
using System.Text.Json.Serialization;
using DSharpPlus.Core.Entities;
using DSharpPlus.Core.Enums;

namespace DSharpPlus.Core.Gateway.Commands
{
    /// <summary>
    /// Used to trigger the initial handshake with the gateway.
    /// </summary>
    public sealed record DiscordIdentifyCommand
    {
        /// <summary>
        /// The authentication token.
        /// </summary>
        [JsonPropertyName("token")]
        public string Token { get; init; } = null!;

        /// <summary>
        /// The connection properties.
        /// </summary>
        [JsonPropertyName("properties")]
        public DiscordIdentifyConnectionProperties Properties { get; init; } = null!;

        /// <summary>
        /// Whether this connection supports compression of packets.
        /// </summary>
        [JsonPropertyName("compress")]
        public Optional<bool> Compress { get; init; } = true;

        /// <summary>
        /// A value between 50 and 250, total number of members where the gateway will stop sending offline members in the guild member list.
        /// </summary>
        [JsonPropertyName("large_threshold")]
        public Optional<int> LargeThreshold { get; init; } = 50;

        /// <summary>
        /// Used for Guild Sharding.
        /// </summary>
        [JsonPropertyName("shard")]
        public Optional<Dictionary<int, int>> Shard { get; init; }

        /// <summary>
        /// The presence structure for initial presence information.
        /// </summary>
        [JsonPropertyName("presence")]
        public Optional<DiscordPresenceUpdateCommand> Presence { get; init; }

        /// <summary>
        /// The <see cref="DiscordGatewayIntents"/> you wish to receive.
        /// </summary>
        [JsonPropertyName("intents")]
        public DiscordGatewayIntents Intents { get; init; }
    }
}
