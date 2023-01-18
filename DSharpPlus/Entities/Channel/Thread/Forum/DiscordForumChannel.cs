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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents either a forum channel or a post in the forum.
    /// </summary>
    public sealed class DiscordForumChannel : DiscordChannel
    {
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public override ChannelType Type => ChannelType.GuildForum;

        /// <summary>
        /// Gets the topic of the forum. This doubles as the guidelines for the forum.
        /// </summary>
        [JsonProperty("topic")]
        public new string Topic { get; internal set; }

        /// <summary>
        /// Gets the default ratelimit per user for the forum. This is applied to all posts upon creation.
        /// </summary>
        [JsonProperty("default_thread_rate_limit_per_user")]
        public int? DefaultPerUserRateLimit { get; internal set; }

        /// <summary>
        /// Gets the available tags for the forum.
        /// </summary>
        public IReadOnlyList<DiscordForumTag> AvailableTags => _availableTags;

        [JsonProperty("available_tags")]
        private List<DiscordForumTag> _availableTags;

        [JsonProperty("default_reaction", NullValueHandling = NullValueHandling.Ignore)]
        public DefaultReaction? DefaultReaction { get; internal set; }

        [JsonProperty("default_sort_order", NullValueHandling = NullValueHandling.Ignore)]
        public DefaultSortOrder? DefaultSortOrder { get; internal set; }

        [JsonProperty("default_forum_layout", NullValueHandling = NullValueHandling.Ignore)]
        public DefaultForumLayout? DefaultForumLayout { get; internal set; }

        /// <summary>
        /// Creates a forum post.
        /// </summary>
        /// <param name="builder">The builder to create the forum post with.</param>
        /// <returns>The starter (the created thread, and the initial message) from creating the post.</returns>
        public Task<DiscordForumPostStarter> CreateForumPostAsync(ForumPostBuilder builder)
            => this.Discord.ApiClient.CreateForumPostAsync(this.Id, builder.Name, builder.AutoArchiveDuration, builder.SlowMode, builder.Message, builder.AppliedTags.Select(t => t.Id));

        internal DiscordForumChannel() {}
    }
}
