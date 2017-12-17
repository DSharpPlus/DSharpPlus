﻿using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace DSharpPlus.CommandsNext.Attributes
{
    /// <summary>
    /// Defines a cooldown for this command. This allows you to define how many times can users execute a specific command
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class CooldownAttribute : CheckBaseAttribute
    {
        /// <summary>
        /// Gets the maximum number of uses before this command triggers a cooldown for its bucket.
        /// </summary>
        public int MaxUses { get; }

        /// <summary>
        /// Gets the time after which the cooldown is reset.
        /// </summary>
        public TimeSpan Reset { get; }

        /// <summary>
        /// Gets the type of the cooldown bucket. This determines how cooldowns are applied.
        /// </summary>
        public CooldownBucketType BucketType { get; }

        /// <summary>
        /// Gets the cooldown buckets for this command.
        /// </summary>
        private ConcurrentDictionary<string, CommandCooldownBucket> Buckets { get; }

        /// <summary>
        /// Defines a cooldown for this command. This means that users will be able to use the command a specific number of times before they have to wait to use it again.
        /// </summary>
        /// <param name="maxUses">Number of times the command can be used before triggering a cooldown.</param>
        /// <param name="resetAfter">Number of seconds after which the cooldown is reset.</param>
        /// <param name="bucketType">Type of cooldown bucket. This allows controlling whether the bucket will be cooled down per user, guild, channel, or globally.</param>
        public CooldownAttribute(int maxUses, double resetAfter, CooldownBucketType bucketType)
        {
            this.MaxUses = maxUses;
            this.Reset = TimeSpan.FromSeconds(resetAfter);
            this.BucketType = bucketType;
            this.Buckets = new ConcurrentDictionary<string, CommandCooldownBucket>();
        }

        /// <summary>
        /// Gets a cooldown bucket for given command context.
        /// </summary>
        /// <param name="ctx">Command context to get cooldown bucket for.</param>
        /// <returns>Requested cooldown bucket, or null if one wasn't present.</returns>
        public CommandCooldownBucket GetBucket(CommandContext ctx)
        {
            var bid = this.GetBucketId(ctx, out _, out _, out _);
            this.Buckets.TryGetValue(bid, out var bucket);
            return bucket;
        }

        /// <summary>
        /// Calculates the cooldown remaining for given command context.
        /// </summary>
        /// <param name="ctx">Context for which to calculate the cooldown.</param>
        /// <returns>Remaining cooldown, or zero if no cooldown is active.</returns>
        public TimeSpan GetRemainingCooldown(CommandContext ctx)
        {
            var bucket = this.GetBucket(ctx);
            if (bucket == null)
                return TimeSpan.Zero;

            if (bucket.RemainingUses > 0)
                return TimeSpan.Zero;

            return bucket.ResetsAt - DateTimeOffset.UtcNow;
        }

        /// <summary>
        /// Calculates bucket ID for given command context.
        /// </summary>
        /// <param name="ctx">Context for which to calculate bucket ID for.</param>
        /// <param name="userId">ID of the user with which this bucket is associated.</param>
        /// <param name="channelId">ID of the channel with which this bucket is associated.</param>
        /// <param name="guildId">ID of the guild with which this bucket is associated.</param>
        /// <returns>Calculated bucket ID.</returns>
        private string GetBucketId(CommandContext ctx, out ulong userId, out ulong channelId, out ulong guildId)
        {
            userId = 0ul;
            if ((this.BucketType & CooldownBucketType.User) != 0)
                userId = ctx.User.Id;

            channelId = 0ul;
            if ((this.BucketType & CooldownBucketType.Channel) != 0)
                channelId = ctx.Channel.Id;
            if ((this.BucketType & CooldownBucketType.Guild) != 0 && ctx.Guild == null)
                channelId = ctx.Channel.Id;

            guildId = 0ul;
            if (ctx.Guild != null && (this.BucketType & CooldownBucketType.Guild) != 0)
                guildId = ctx.Guild.Id;

            var bid = CommandCooldownBucket.MakeId(userId, channelId, guildId);
            return bid;
        }

        public override async Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        {
            if (help)
                return true;

            var bid = this.GetBucketId(ctx, out var usr, out var chn, out var gld);
            if (!this.Buckets.TryGetValue(bid, out var bucket))
            {
                bucket = new CommandCooldownBucket(this.MaxUses, this.Reset, usr, chn, gld);
                this.Buckets.AddOrUpdate(bid, bucket, (k, v) => bucket);
            }

            return await bucket.DecrementUseAsync().ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Defines how are command cooldowns applied.
    /// </summary>
    [Flags]
    public enum CooldownBucketType
    {
        /// <summary>
        /// Denotes that the command will have its cooldown applied per-user.
        /// </summary>
        User = 1,

        /// <summary>
        /// Denotes that the command will have its cooldown applied per-channel.
        /// </summary>
        Channel = 2,

        /// <summary>
        /// Denotes that the command will have its cooldown applied per-guild. In DMs, this applies the cooldown per-channel.
        /// </summary>
        Guild = 4,

        /// <summary>
        /// Denotes that the command will have its cooldown applied globally.
        /// </summary>
        Global = 0
    }

    /// <summary>
    /// Represents a cooldown bucket for commands.
    /// </summary>
    public sealed class CommandCooldownBucket : IEquatable<CommandCooldownBucket>
    {
        /// <summary>
        /// Gets the ID of the user with whom this cooldown is associated.
        /// </summary>
        public ulong UserId { get; }

        /// <summary>
        /// Gets the ID of the channel with which this cooldown is associated.
        /// </summary>
        public ulong ChannelId { get; }

        /// <summary>
        /// Gets the ID of the guild with which this cooldown is associated.
        /// </summary>
        public ulong GuildId { get; }

        /// <summary>
        /// Gets the ID of the bucket. This is used to distinguish between cooldown buckets.
        /// </summary>
        public string BucketId { get; }

        /// <summary>
        /// Gets the remaining number of uses before the cooldown is triggered.
        /// </summary>
        public int RemainingUses 
            => Volatile.Read(ref this._remaining_uses);

        private int _remaining_uses;

        /// <summary>
        /// Gets the maximum number of times this command can be used in given timespan.
        /// </summary>
        public int MaxUses { get; }

        /// <summary>
        /// Gets the date and time at which the cooldown resets.
        /// </summary>
        public DateTimeOffset ResetsAt { get; internal set; }

        /// <summary>
        /// Gets the time after which this cooldown resets.
        /// </summary>
        public TimeSpan Reset { get; internal set; }

        /// <summary>
        /// Gets the semaphore used to lock the use value.
        /// </summary>
        private SemaphoreSlim UsageSemaphore { get; }

        /// <summary>
        /// Creates a new command cooldown bucket.
        /// </summary>
        /// <param name="maxUses">Maximum number of uses for this bucket.</param>
        /// <param name="resetAfter">Time after which this bucket resets.</param>
        /// <param name="userId">ID of the user with which this cooldown is associated.</param>
        /// <param name="channelId">ID of the channel with which this cooldown is associated.</param>
        /// <param name="guildId">ID of the guild with which this cooldown is associated.</param>
        internal CommandCooldownBucket(int maxUses, TimeSpan resetAfter, ulong userId = 0, ulong channelId = 0, ulong guildId = 0)
        {
            this._remaining_uses = maxUses;
            this.MaxUses = maxUses;
            this.ResetsAt = DateTimeOffset.UtcNow + resetAfter;
            this.Reset = resetAfter;
            this.UserId = userId;
            this.ChannelId = channelId;
            this.GuildId = guildId;
            this.BucketId = MakeId(userId, channelId, guildId);
            this.UsageSemaphore = new SemaphoreSlim(1, 1);
        }

        /// <summary>
        /// Decrements the remaining use counter.
        /// </summary>
        /// <returns>Whether decrement succeded or not.</returns>
        internal async Task<bool> DecrementUseAsync()
        {
            await this.UsageSemaphore.WaitAsync().ConfigureAwait(false);

            // if we're past reset time...
            var now = DateTimeOffset.UtcNow;
            if (now >= this.ResetsAt)
            {
                // ...do the reset and set a new reset time
                Interlocked.Exchange(ref this._remaining_uses, this.MaxUses);
                this.ResetsAt = now + this.Reset;
            }

            // check if we have any uses left, if we do...
            var success = false;
            if (this.RemainingUses > 0)
            {
                // ...decrement, and return success...
                Interlocked.Decrement(ref this._remaining_uses);
                success = true;
            }

            // ...otherwise just fail
            this.UsageSemaphore.Release();
            return success;
        }

        /// <summary>
        /// Returns a string representation of this command cooldown bucket.
        /// </summary>
        /// <returns>String representation of this command cooldown bucket.</returns>
        public override string ToString()
        {
            return $"Command bucket {this.BucketId}";
        }

        /// <summary>
        /// Checks whether this <see cref="CommandCooldownBucket"/> is equal to another object.
        /// </summary>
        /// <param name="obj">Object to compare to.</param>
        /// <returns>Whether the object is equal to this <see cref="CommandCooldownBucket"/>.</returns>
        public override bool Equals(object obj)
        {
            return this.Equals(obj as CommandCooldownBucket);
        }

        /// <summary>
        /// Checks whether this <see cref="CommandCooldownBucket"/> is equal to another <see cref="CommandCooldownBucket"/>.
        /// </summary>
        /// <param name="other"><see cref="CommandCooldownBucket"/> to compare to.</param>
        /// <returns>Whether the <see cref="CommandCooldownBucket"/> is equal to this <see cref="CommandCooldownBucket"/>.</returns>
        public bool Equals(CommandCooldownBucket other)
        {
            if (ReferenceEquals(other, null))
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return this.UserId == other.UserId && this.ChannelId == other.ChannelId && this.GuildId == other.GuildId;
        }

        /// <summary>
        /// Gets the hash code for this <see cref="CommandCooldownBucket"/>.
        /// </summary>
        /// <returns>The hash code for this <see cref="CommandCooldownBucket"/>.</returns>
        public override int GetHashCode()
        {
            int hash = 13;

            hash = (hash * 7) + this.UserId.GetHashCode();
            hash = (hash * 7) + this.ChannelId.GetHashCode();
            hash = (hash * 7) + this.GuildId.GetHashCode();

            return hash;
        }

        /// <summary>
        /// Gets whether the two <see cref="CommandCooldownBucket"/> objects are equal.
        /// </summary>
        /// <param name="bucket1">First bucket to compare.</param>
        /// <param name="bucket2">Second bucket to compare.</param>
        /// <returns>Whether the two buckets are equal.</returns>
        public static bool operator ==(CommandCooldownBucket bucket1, CommandCooldownBucket bucket2)
        {
            var null1 = ReferenceEquals(bucket1, null);
            var null2 = ReferenceEquals(bucket2, null);

            if (null1 && null2)
                return true;

            if (null1 != null2)
                return false;

            return null1.Equals(null2);
        }

        /// <summary>
        /// Gets whether the two <see cref="CommandCooldownBucket"/> objects are not equal.
        /// </summary>
        /// <param name="bucket1">First bucket to compare.</param>
        /// <param name="bucket2">Second bucket to compare.</param>
        /// <returns>Whether the two buckets are not equal.</returns>
        public static bool operator !=(CommandCooldownBucket bucket1, CommandCooldownBucket bucket2) 
            => !(bucket1 == bucket2);

        /// <summary>
        /// Creates a bucket ID from given bucket parameters.
        /// </summary>
        /// <param name="userId">ID of the user with which this cooldown is associated.</param>
        /// <param name="channelId">ID of the channel with which this cooldown is associated.</param>
        /// <param name="guildId">ID of the guild with which this cooldown is associated.</param>
        /// <returns>Generated bucket ID.</returns>
        public static string MakeId(ulong userId = 0, ulong channelId = 0, ulong guildId = 0) 
            => $"{userId.ToString(CultureInfo.InvariantCulture)}:{channelId.ToString(CultureInfo.InvariantCulture)}:{guildId.ToString(CultureInfo.InvariantCulture)}";
    }
}
