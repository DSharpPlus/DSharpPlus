// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2023 DSharpPlus Contributors
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

using DSharpPlus.Entities;
using Newtonsoft.Json;

namespace DSharpPlus.Net.Abstractions
{
    /// <summary>
    /// Represents data for websocket status update payload.
    /// </summary>
    internal sealed class StatusUpdate
    {
        /// <summary>
        /// Gets or sets the unix millisecond timestamp of when the user went idle.
        /// </summary>
        [JsonProperty("since", NullValueHandling = NullValueHandling.Include)]
        public long? IdleSince { get; set; }

        /// <summary>
        /// Gets or sets whether the user is AFK.
        /// </summary>
        [JsonProperty("afk")]
        public bool IsAFK { get; set; }

        /// <summary>
        /// Gets or sets the status of the user.
        /// </summary>
        [JsonIgnore]
        public UserStatus Status { get; set; } = UserStatus.Online;

        [JsonProperty("status")]
        internal string StatusString
        {
            get
            {
                return this.Status switch
                {
                    UserStatus.Online => "online",
                    UserStatus.Idle => "idle",
                    UserStatus.DoNotDisturb => "dnd",
                    UserStatus.Invisible or UserStatus.Offline => "invisible",
                    _ => "online",
                };
            }
        }

        /// <summary>
        /// Gets or sets the game the user is playing.
        /// </summary>
        [JsonProperty("game", NullValueHandling = NullValueHandling.Ignore)]
        public TransportActivity Activity { get; set; }

        internal DiscordActivity _activity;
    }
}
