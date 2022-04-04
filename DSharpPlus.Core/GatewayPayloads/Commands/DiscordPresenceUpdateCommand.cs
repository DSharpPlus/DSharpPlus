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

using DSharpPlus.Core.Entities;
using DSharpPlus.Core.Enums;
using Newtonsoft.Json;

namespace DSharpPlus.Core.Gateway.Commands
{
    /// <summary>
    /// Sent by the client to indicate a presence or status update.
    /// </summary>
    public sealed record DiscordPresenceUpdateCommand
    {
        /// <summary>
        /// The unix time (in milliseconds) of when the client went idle, or null if the client is not idle.
        /// </summary>
        [JsonProperty("since", NullValueHandling = NullValueHandling.Ignore)]
        public int? Since { get; internal set; }

        /// <summary>
        /// The user's activities.
        /// </summary>
        [JsonProperty("activities", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordActivity[] Activities { get; internal set; } = null!;

        /// <summary>
        /// The user's new status.
        /// </summary>
        [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordStatusType Status { get; internal set; }

        /// <summary>
        /// Whether or not the client is afk.
        /// </summary>
        [JsonProperty("afk", NullValueHandling = NullValueHandling.Ignore)]
        public bool AFK { get; internal set; }
    }
}
