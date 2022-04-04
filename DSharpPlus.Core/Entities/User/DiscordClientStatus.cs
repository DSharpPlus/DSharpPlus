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

using Newtonsoft.Json;

namespace DSharpPlus.Core.Entities
{
    /// <summary>
    /// Active sessions are indicated with an "online", "idle", or "dnd" string per platform. If a user is offline or invisible, the corresponding field is not present.
    /// </summary>
    public sealed record DiscordClientStatus
    {
        /// <summary>
        /// The user's status set for an active desktop (Windows, Linux, Mac) application session.
        /// </summary>
        [JsonProperty("desktop", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<string> Desktop { get; init; }

        /// <summary>
        /// The user's status set for an active mobile (iOS, Android) application session.
        /// </summary>
        [JsonProperty("mobile", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<string> Mobile { get; init; }

        /// <summary>
        /// The user's status set for an active web (browser, bot account) application session.
        /// </summary>
        [JsonProperty("web", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<string> Web { get; init; }
    }
}
