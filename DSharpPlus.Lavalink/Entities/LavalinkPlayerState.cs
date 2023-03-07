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

namespace DSharpPlus.Lavalink.Entities
{
    public class LavalinkPlayerState
    {

        /// <summary>
        /// Unix timestamp in milliseconds
        /// </summary>
        public DateTimeOffset Time => DateTimeOffset.FromUnixTimeMilliseconds(this._time);
        [JsonProperty("time")] private readonly long _time;

        /// <summary>
        /// The position of the track in milliseconds
        /// </summary>
        [JsonIgnore]
        public TimeSpan Position {
            get => TimeSpan.FromMilliseconds(this._position);
            internal set => this._position = (long)value.TotalMilliseconds;
        }
        [JsonProperty("position")]
        internal long _position { get; set; }

        /// <summary>
        /// If Lavalink is connected to the voice gateway
        /// </summary>
        [JsonProperty("connected")]
        public bool Connected { get; internal set; }

        /// <summary>
        /// The ping of the node to the Discord voice server in milliseconds (-1 if not connected)
        /// </summary>
        [JsonProperty("ping")]
        public int Ping { get; internal set; }

        /// <summary>
        /// Current track that playing on the player
        /// </summary>
        [JsonIgnore]
        public LavalinkTrack Track { get; internal set; }
    }
}
