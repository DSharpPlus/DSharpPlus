using System;
using System.Collections.Generic;
using System.Linq;
namespace DSharpPlus.Entities
{
    /// <summary>
    /// A builder to create a forum post.
    /// </summary>
    public class ForumPostBuilder
    {
        /// <summary>
        /// The name (or title) of the post.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The time (in seconds) that users must wait between messages.
        /// </summary>
        public int? SlowMode { get; set; }

        /// <summary>
        /// The message to initiate the forum post with.
        /// </summary>
        public DiscordMessageBuilder Message { get; set; }

        /// <summary>
        /// The tags to apply to this post.
        /// </summary>
        public IReadOnlyList<DiscordForumTag> AppliedTags { get; }

        /// <summary>
        /// When to automatically archive the post.
        /// </summary>
        public AutoArchiveDuration? AutoArchiveDuration { get; set; }

        /// <summary>
        /// Creates a new forum post builder.
        /// </summary>
        public ForumPostBuilder()
        {
           AppliedTags = new List<DiscordForumTag>();
        }

        /// <summary>
        /// Sets the name (or title) of the post.
        /// </summary>
        /// <param name="name">The name of the post.</param>
        /// <returns>The builder to chain calls with</returns>
        public ForumPostBuilder WithName(string name)
        {
            this.Name = name;
            return this;
        }

        /// <summary>
        /// Sets slowmode for the post.
        /// </summary>
        /// <param name="slowMode">The time in seconds to apply</param>
        /// <returns></returns>
        public ForumPostBuilder WithSlowMode(int slowMode)
        {
            this.SlowMode = slowMode;
            return this;
        }

        /// <summary>
        /// Sets slow mode for the post.
        /// </summary>
        /// <param name="slowMode">The slowmode delay to set.</param>
        /// <returns>The builder to chain calls with.</returns>
        public ForumPostBuilder WithSlowMode(TimeSpan slowMode)
        {
            this.SlowMode = (int)slowMode.TotalSeconds;
            return this;
        }

        /// <summary>
        /// Sets the message to initiate the forum post with.
        /// </summary>
        /// <param name="message">The message to start the post with.</param>
        /// <returns>The builder to chain calls with.</returns>
        public ForumPostBuilder WithMessage(DiscordMessageBuilder message)
        {
            this.Message = message;
            return this;
        }

        /// <summary>
        /// Sets the auto archive duration for the post.
        /// </summary>
        /// <param name="autoArchiveDuration">The duration in which the post will automatically archive</param>
        /// <returns>The builder to chain calls with</returns>
        public ForumPostBuilder WithAutoArchiveDuration(AutoArchiveDuration autoArchiveDuration)
        {
            this.AutoArchiveDuration = autoArchiveDuration;
            return this;
        }

        /// <summary>
        /// Adds a tag to the post.
        /// </summary>
        /// <param name="tag">The tag to add.</param>
        /// <returns>The builder to chain calls with.</returns>
        public ForumPostBuilder AddTag(DiscordForumTag tag)
        {
            ((List<DiscordForumTag>)this.AppliedTags).Add(tag);
            return this;
        }

        /// <summary>
        /// Adds several tags to the post.
        /// </summary>
        /// <param name="tags">The tags to add.</param>
        /// <returns>The builder to chain calls with.</returns>
        public ForumPostBuilder AddTags(IEnumerable<DiscordForumTag> tags)
        {
            ((List<DiscordForumTag>)this.AppliedTags).AddRange(tags);
            return this;
        }

        /// <summary>
        /// Removes a tag from the post.
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public ForumPostBuilder RemoveTag(DiscordForumTag tag)
        {
            ((List<DiscordForumTag>)this.AppliedTags).Remove(tag);
            return this;
        }
    }
}
