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

        /// <summary>
        /// The default reaction shown on posts when they are created.
        /// </summary>
        [JsonProperty("default_reaction_emoji", NullValueHandling = NullValueHandling.Ignore)]
        public DefaultReaction? DefaultReaction { get; internal set; }

        /// <summary>
        /// The default sort order of posts in the forum.
        /// </summary>
        [JsonProperty("default_sort_order", NullValueHandling = NullValueHandling.Ignore)]
        public DefaultSortOrder? DefaultSortOrder { get; internal set; }

        /// <summary>
        /// The default layout of posts in the forum. Defaults to <see cref="DefaultForumLayout.ListView"/>
        /// </summary>
        [JsonProperty("default_forum_layout", NullValueHandling = NullValueHandling.Ignore)]
        public DefaultForumLayout? DefaultLayout { get; internal set; }

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
