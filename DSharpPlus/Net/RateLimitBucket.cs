using System;

namespace DSharpPlus.Net
{
    /// <summary>
    /// Represents a rate limit bucket.
    /// </summary>
    internal class RateLimitBucket : IEquatable<RateLimitBucket>
    {
        /// <summary>
        /// Gets the Id of the guild bucket.
        /// </summary>
        public string GuildId { get; internal set; }

        /// <summary>
        /// Gets the Id of the channel bucket.
        /// </summary>
        public string ChannelId { get; internal set; }

        /// <summary>
        /// Gets the url by which the requests are bucketed.
        /// </summary>
        public string Route { get; internal set; }

        /// <summary>
        /// Gets the HTTP request method.
        /// </summary>
        public RestRequestMethod Method { get; internal set; }

        /// <summary>
        /// Gets the Id of the ratelimit bucket.
        /// </summary>
        public string BucketId => $"{Method}:{GuildId}:{ChannelId}:{Route}";

        /// <summary>
        /// Gets the number of uses left before pre-emptive rate limit is triggered.
        /// </summary>
        public int Remaining { get; internal set; }

        /// <summary>
        /// Gets the maximum number of uses within a single bucket.
        /// </summary>
        public int Maximum { get; internal set; }

        /// <summary>
        /// Gets the timestamp at which the rate limit resets.
        /// </summary>
        public DateTimeOffset Reset { get; internal set; }

        internal RateLimitBucket(RestRequestMethod method, string route, string guildId, string channelId)
        {
            Method = method;
            Route = route;
            ChannelId = channelId;
            GuildId = guildId;
        }

        /// <summary>
        /// Generates an ID for this request bucket.
        /// </summary>
        /// <param name="method">Method for this bucket.</param>
        /// <param name="route">Route for this bucket.</param>
        /// <param name="guildId">Guild Id for this bucket.</param>
        /// <param name="channelId">Channel Id for this bucket.</param>
        /// <returns>Bucket Id.</returns>
        public static string GenerateId(RestRequestMethod method, string route, string guildId, string channelId) =>
            $"{method}:{guildId}:{channelId}:{route}";

        /// <summary>
        /// Returns a string representation of this bucket.
        /// </summary>
        /// <returns>String representation of this bucket.</returns>
        public override string ToString()
        {
            return $"Rate limit bucket [{Method}:{GuildId}:{ChannelId}:{Route}] [{Remaining}/{Maximum}] {Reset}";
        }

        /// <summary>
        /// Checks whether this <see cref="RateLimitBucket"/> is equal to another object.
        /// </summary>
        /// <param name="obj">Object to compare to.</param>
        /// <returns>Whether the object is equal to this <see cref="RateLimitBucket"/>.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as RateLimitBucket);
        }

        /// <summary>
        /// Checks whether this <see cref="RateLimitBucket"/> is equal to another <see cref="RateLimitBucket"/>.
        /// </summary>
        /// <param name="e"><see cref="RateLimitBucket"/> to compare to.</param>
        /// <returns>Whether the <see cref="RateLimitBucket"/> is equal to this <see cref="RateLimitBucket"/>.</returns>
        public bool Equals(RateLimitBucket e)
        {
            if (ReferenceEquals(e, null))
            {
                return false;
            }

            if (ReferenceEquals(this, e))
            {
                return true;
            }

            return BucketId == e.BucketId;
        }

        /// <summary>
        /// Gets the hash code for this <see cref="RateLimitBucket"/>.
        /// </summary>
        /// <returns>The hash code for this <see cref="RateLimitBucket"/>.</returns>
        public override int GetHashCode()
        {
            return BucketId.GetHashCode();
        }
    }
}
