﻿using System;
using System.Collections.Concurrent;
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
        /// Gets the Id of the ratelimit bucket.
        /// </summary>
        public volatile string BucketId;

        /// <summary>
        /// Gets or sets the ratelimit hash of this bucket.
        /// </summary>
        public string Hash 
        { 
            get => Volatile.Read(ref this._hash);

            internal set
            {
                if (value.Contains(UNLIMITED_HASH))
                    this.IsUnlimited = true;
                else
                    this.IsUnlimited = false;

                if (this.BucketId != null && !this.BucketId.StartsWith(value))
                {
                    var id = GenerateBucketId(value, this.GuildId, this.ChannelId, this.WebhookId);
                    this.BucketId = id;
                    this.RouteHashes.Add(id);
                }

                Volatile.Write(ref this._hash, value);
            }
        }

        internal string _hash;

        /// <summary>
        /// Gets the past route hashes associated with this bucket.
        /// </summary>
        public ConcurrentBag<string> RouteHashes { get; }

        /// <summary>
        /// Gets when this bucket was last called in a request.
        /// </summary>
        public DateTimeOffset LastAttemptAt { get; internal set; }

        /// <summary>
        /// Gets the number of uses left before pre-emptive rate limit is triggered.
        /// </summary>
        public int Remaining 
            => this._remaining;

        /// <summary>
        /// Gets the maximum number of uses within a single bucket.
        /// </summary>
        public int Maximum { get; set; }

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
        /// Gets whether this bucket has it's ratelimit determined.
        /// <para>This will be <see langword="false"/> if the ratelimit is determined.</para>
        /// </summary>
        internal volatile bool IsUnlimited;

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

        private static readonly string UNLIMITED_HASH = "unlimited";

        internal RateLimitBucket(string hash, string guild_id, string channel_id, string webhook_id)
        {
            this.Hash = hash;
            this.ChannelId = channel_id;
            this.GuildId = guild_id;
            this.WebhookId = webhook_id;

            this.BucketId = GenerateBucketId(hash, guild_id, channel_id, webhook_id);
            this.RouteHashes = new ConcurrentBag<string>();
        }

        /// <summary>
        /// Generates an ID for this request bucket.
        /// </summary>
        /// <param name="hash">Hash for this bucket.</param>
        /// <param name="guild_id">Guild Id for this bucket.</param>
        /// <param name="channel_id">Channel Id for this bucket.</param>
        /// <param name="webhook_id">Webhook Id for this bucket.</param>
        /// <returns>Bucket Id.</returns>
        public static string GenerateBucketId(string hash, string guild_id, string channel_id, string webhook_id)
            => $"{hash}:{guild_id}:{channel_id}:{webhook_id}";

        public static string GenerateHashKey(RestRequestMethod method, string route)
            => $"{method}:{route}";

        public static string GenerateUnlimitedHash(RestRequestMethod method, string route)
            => $"{GenerateHashKey(method, route)}:{UNLIMITED_HASH}";

        /// <summary>
        /// Returns a string representation of this bucket.
        /// </summary>
        /// <returns>String representation of this bucket.</returns>
        public override string ToString()
        {
            var guildId = this.GuildId != string.Empty ? this.GuildId : "guild_id";
            var channelId = this.ChannelId != string.Empty ? this.ChannelId : "channel_id";
            var webhookId = this.WebhookId != string.Empty ? this.WebhookId : "webhook_id";

            return $"rate limit bucket [{this.Hash}:{guildId}:{channelId}:{webhookId}] [{this.Remaining}/{this.Maximum}] {(this.ResetAfter.HasValue ? this._resetAfterOffset : this.Reset)}";
        }

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

        internal void SetInitialValues(int max, int usesLeft, DateTimeOffset newReset)
        {
            this.Maximum = max;
            this._remaining = usesLeft;
            this._nextReset = newReset.UtcTicks;

            this._limitValid = true;
            this._limitTestFinished = null;
            this._limitTesting = 0;
        }
    }
}
