// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2021 DSharpPlus Contributors
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Exceptions;
using DSharpPlus.Net.Abstractions;
using DSharpPlus.Net.Models;
using Newtonsoft.Json;

namespace DSharpPlus.Entities
{
    public class DiscordThreadChannelMember
    {
        /// <summary>
        /// Gets ID of the thread.
        /// </summary>
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong ThreadId { get; set; }
        
        /// <summary>
        /// Gets ID of the user.
        /// </summary>
        [JsonProperty("user_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong Id { get; set; }

        /// <summary>
        /// Gets timestamp when the user joined the thread.
        /// </summary>
        [JsonIgnore]
        public DateTimeOffset? JoinTimeStamp
            => !string.IsNullOrWhiteSpace(this.JoinTimeStampRaw) && DateTimeOffset.TryParse(this.JoinTimeStampRaw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dto) ?
                dto : null;

        [JsonProperty("join_timestamp", NullValueHandling = NullValueHandling.Ignore)]
        internal string JoinTimeStampRaw { get; set; }

        [JsonProperty("flags", NullValueHandling = NullValueHandling.Ignore)]
        internal int UserFlags { get; set; }

        /// <summary>
        /// Gets the DiscordMember that represents this ThreadMember. Can be a skeleton object.
        /// </summary>
        [JsonIgnore]
        public DiscordMember Member
            => this.Guild != null ? (this.Guild._members.TryGetValue(this.Id, out var member) ? member : new DiscordMember { Id = Id, _guild_id = _guild_id, Discord = Discord}) : null;

        /// <summary>
        /// Gets the category that contains this channel. For threads, gets the channel this thread was created in.
        /// </summary>
        [JsonIgnore]
        public DiscordChannel Thread
            => this.Guild != null ? (this.Guild._threads.TryGetValue(this.ThreadId, out var thread) ? thread : null) : null;

        /// <summary>
        /// Gets the guild to which this channel belongs.
        /// </summary>
        [JsonIgnore]
        public DiscordGuild Guild
            => this.Discord.Guilds.TryGetValue(this._guild_id, out var guild) ? guild : null;

        [JsonIgnore]
        internal ulong _guild_id;

        /// <summary>
        /// Gets the client instance this object is tied to.
        /// </summary>
        [JsonIgnore]
        internal BaseDiscordClient Discord { get; set; }

        internal DiscordThreadChannelMember() { }
    }
}
