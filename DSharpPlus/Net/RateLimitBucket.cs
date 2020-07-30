using System;
using System.Threading;
using System.Threading.Tasks;

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
        /// Gets the ID of the webhook bucket.
        /// </summary>
        public string WebhookId { get; internal set; }

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
        public string BucketId 
            => $"{this.Method}:{this.GuildId}:{this.ChannelId}:{this.WebhookId}:{this.Route}";

        /// <summary>
        /// Gets the number of uses left before pre-emptive rate limit is triggered.
        /// </summary>
        public int Remaining => _remaining;

        /// <summary>
        /// Gets the maximum number of uses within a single bucket.
        /// </summary>
        public int Maximum { get; internal set; }

        /// <summary>
        /// Gets the timestamp at which the rate limit resets.
        /// </summary>
        public DateTimeOffset Reset { get; internal set; }

        /// <summary>
        /// Gets the time interval to wait before the rate limit resets.
        /// </summary>
        public TimeSpan? ResetAfter { get; internal set; } = null;

        internal DateTimeOffset _resetAfterOffset { get; set; }

        internal volatile int _remaining;

        /// <summary>
        /// If the initial request for this bucket that is deterternining the rate limits is currently executing
        /// This is a int because booleans can't be accessed atomically
        /// 0 => False, all other values => True
        /// </summary>
        internal volatile int _limitTesting;

        /// <summary>
        /// Task to wait for the rate limit test to finish
        /// </summary>
        internal volatile Task _limitTestFinished;

        /// <summary>
        /// If the rate limits have been determined
        /// </summary>
        internal volatile bool _limitValid;

        /// <summary>
        /// Rate limit reset in ticks, UTC on the next response after the rate limit has been reset
        /// </summary>
        internal long _nextReset;
        
        /// <summary>
        /// If the rate limit is currently being reset.
        /// This is a int because booleans can't be accessed atomically.
        /// 0 => False, all other values => True
        /// </summary>
        internal volatile int _limitResetting;

        internal RateLimitBucket(RestRequestMethod method, string route, string guild_id, string channel_id, string webhook_id)
        {
            this.Method = method;
            this.Route = route;
            this.ChannelId = channel_id;
            this.GuildId = guild_id;
            this.WebhookId = webhook_id;
        }

        /// <summary>
        /// Generates an ID for this request bucket.
        /// </summary>
        /// <param name="method">Method for this bucket.</param>
        /// <param name="route">Route for this bucket.</param>
        /// <param name="guild_id">Guild Id for this bucket.</param>
        /// <param name="channel_id">Channel Id for this bucket.</param>
        /// <param name="webhook_id">Webhook Id for this bucket.</param>
        /// <returns>Bucket Id.</returns>
        public static string GenerateId(RestRequestMethod method, string route, string guild_id, string channel_id, string webhook_id) 
            => $"{method}:{guild_id}:{channel_id}:{webhook_id}:{route}";

        /// <summary>
        /// Returns a string representation of this bucket.
        /// </summary>
        /// <returns>String representation of this bucket.</returns>
        public override string ToString()
            => $"rate limit bucket [{this.Method}:{this.GuildId}:{this.ChannelId}:{this.WebhookId}:{this.Route}] [{this.Remaining}/{this.Maximum}] {(this.ResetAfter.HasValue ? this._resetAfterOffset : this.Reset)}";

        /// <summary>
        /// Checks whether this <see cref="RateLimitBucket"/> is equal to another object.
        /// </summary>
        /// <param name="obj">Object to compare to.</param>
        /// <returns>Whether the object is equal to this <see cref="RateLimitBucket"/>.</returns>
        public override bool Equals(object obj)
            => this.Equals(obj as RateLimitBucket);

        /// <summary>
        /// Checks whether this <see cref="RateLimitBucket"/> is equal to another <see cref="RateLimitBucket"/>.
        /// </summary>
        /// <param name="e"><see cref="RateLimitBucket"/> to compare to.</param>
        /// <returns>Whether the <see cref="RateLimitBucket"/> is equal to this <see cref="RateLimitBucket"/>.</returns>
        public bool Equals(RateLimitBucket e)
        {
            if (ReferenceEquals(e, null))
                return false;

            if (ReferenceEquals(this, e))
                return true;

            return this.BucketId == e.BucketId;
        }

        /// <summary>
        /// Gets the hash code for this <see cref="RateLimitBucket"/>.
        /// </summary>
        /// <returns>The hash code for this <see cref="RateLimitBucket"/>.</returns>
        public override int GetHashCode()
            => this.BucketId.GetHashCode();

        /// <summary>
        /// Sets remaining number of requests to the maximum when the ratelimit is reset
        /// </summary>
        /// <param name="now"></param>
        internal async Task TryResetLimitAsync(DateTimeOffset now)
        {
            if (this.ResetAfter.HasValue)
                this.ResetAfter = this._resetAfterOffset - now;

            if (this._nextReset == 0)
                return;

            if (this._nextReset > now.UtcTicks)
                return;

            while (Interlocked.CompareExchange(ref this._limitResetting, 1, 0) != 0)
#pragma warning restore 420
                await Task.Yield();

            if (this._nextReset != 0)
            {
                this._remaining = this.Maximum;
                this._nextReset = 0;
            }

            this._limitResetting = 0;
        }

        internal void SetInitialValues(int usesLeft, DateTimeOffset newReset)
        {
            this._remaining = usesLeft;
            this._nextReset = newReset.UtcTicks;
            this._limitValid = true;
            this._limitTestFinished = null;
            this._limitTesting = 0;
        }
    }
}
