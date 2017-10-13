using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace DSharpPlus.CommandsNext.Attributes
{
    /// <summary>
    /// Defines a cooldown for this command. This allows you to define how many times can users execute a specific command
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
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
        /// <param name="reset">Number of seconds after which the cooldown is reset.</param>
        /// <param name="bucketType">Type of cooldown bucket. This allows controlling whether the bucket will be cooled down per user, guild, channel, or globally.</param>
        public CooldownAttribute(int maxUses, double reset, CooldownBucketType bucketType)
        {
            MaxUses = maxUses;
            Reset = TimeSpan.FromSeconds(reset);
            BucketType = bucketType;
            Buckets = new ConcurrentDictionary<string, CommandCooldownBucket>();
        }

        /// <summary>
        /// Gets a cooldown bucket for given command context.
        /// </summary>
        /// <param name="ctx">Command context to get cooldown bucket for.</param>
        /// <returns>Requested cooldown bucket, or null if one wasn't present.</returns>
        public CommandCooldownBucket GetBucket(CommandContext ctx)
        {
            var bid = GetBucketId(ctx, out _, out _, out _);
            Buckets.TryGetValue(bid, out var bucket);
            return bucket;
        }

        /// <summary>
        /// Calculates the cooldown remaining for given command context.
        /// </summary>
        /// <param name="ctx">Context for which to calculate the cooldown.</param>
        /// <returns>Remaining cooldown, or zero if no cooldown is active.</returns>
        public TimeSpan GetRemainingCooldown(CommandContext ctx)
        {
            var bucket = GetBucket(ctx);
            if (bucket == null)
            {
                return TimeSpan.Zero;
            }

            if (bucket.RemainingUses > 0)
            {
                return TimeSpan.Zero;
            }

            return bucket.ResetsAt - DateTimeOffset.UtcNow;
        }

        /// <summary>
        /// Calculates bucket ID for given command context.
        /// </summary>
        /// <param name="ctx">Context for which to calculate bucket ID for.</param>
        /// <param name="usr">ID of the user with which this bucket is associated.</param>
        /// <param name="chn">ID of the channel with which this bucket is associated.</param>
        /// <param name="gld">ID of the guild with which this bucket is associated.</param>
        /// <returns>Calculated bucket ID.</returns>
        private string GetBucketId(CommandContext ctx, out ulong usr, out ulong chn, out ulong gld)
        {
            usr = 0ul;
            if ((BucketType & CooldownBucketType.User) != 0)
            {
                usr = ctx.User.Id;
            }

            chn = 0ul;
            if ((BucketType & CooldownBucketType.Channel) != 0)
            {
                chn = ctx.Channel.Id;
            }
            if ((BucketType & CooldownBucketType.Guild) != 0 && ctx.Guild == null)
            {
                chn = ctx.Channel.Id;
            }

            gld = 0ul;
            if (ctx.Guild != null && (BucketType & CooldownBucketType.Guild) != 0)
            {
                gld = ctx.Guild.Id;
            }

            var bid = CommandCooldownBucket.MakeId(usr, chn, gld);
            return bid;
        }

        public override async Task<bool> CanExecute(CommandContext ctx, bool help)
        {
            if (help)
            {
                return true;
            }

            var bid = GetBucketId(ctx, out var usr, out var chn, out var gld);
            if (!Buckets.TryGetValue(bid, out var bucket))
            {
                bucket = new CommandCooldownBucket(MaxUses, Reset, usr, chn, gld);
                Buckets.AddOrUpdate(bid, bucket, (k, v) => bucket);
            }

            return await bucket.DecrementUseAsync();
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
        public int RemainingUses => Volatile.Read(ref _remainingUses);
        private int _remainingUses;

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
        /// <param name="uses">Maximum number of uses for this bucket.</param>
        /// <param name="reset">Time after which this bucket resets.</param>
        /// <param name="user">ID of the user with which this cooldown is associated.</param>
        /// <param name="channel">ID of the channel with which this cooldown is associated.</param>
        /// <param name="guild">ID of the guild with which this cooldown is associated.</param>
        internal CommandCooldownBucket(int uses, TimeSpan reset, ulong user = 0, ulong channel = 0, ulong guild = 0)
        {
            _remainingUses = uses;
            MaxUses = uses;
            ResetsAt = DateTimeOffset.UtcNow + reset;
            Reset = reset;
            UserId = user;
            ChannelId = channel;
            GuildId = guild;
            BucketId = MakeId(user, channel, guild);
            UsageSemaphore = new SemaphoreSlim(1, 1);
        }

        /// <summary>
        /// Decrements the remaining use counter.
        /// </summary>
        /// <returns>Whether decrement succeded or not.</returns>
        internal async Task<bool> DecrementUseAsync()
        {
            await UsageSemaphore.WaitAsync();

            // if we're past reset time...
            var now = DateTimeOffset.UtcNow;
            if (now >= ResetsAt)
            {
                // ...do the reset and set a new reset time
                Interlocked.Exchange(ref _remainingUses, MaxUses);
                ResetsAt = now + Reset;
            }

            // check if we have any uses left, if we do...
            var success = false;
            if (RemainingUses > 0)
            {
                // ...decrement, and return success...
                Interlocked.Decrement(ref _remainingUses);
                success = true;
            }

            // ...otherwise just fail
            UsageSemaphore.Release();
            return success;
        }

        /// <summary>
        /// Returns a string representation of this command cooldown bucket.
        /// </summary>
        /// <returns>String representation of this command cooldown bucket.</returns>
        public override string ToString()
        {
            return $"Command bucket {BucketId}";
        }

        /// <summary>
        /// Checks whether this <see cref="CommandCooldownBucket"/> is equal to another object.
        /// </summary>
        /// <param name="obj">Object to compare to.</param>
        /// <returns>Whether the object is equal to this <see cref="CommandCooldownBucket"/>.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as CommandCooldownBucket);
        }

        /// <summary>
        /// Checks whether this <see cref="CommandCooldownBucket"/> is equal to another <see cref="CommandCooldownBucket"/>.
        /// </summary>
        /// <param name="other"><see cref="CommandCooldownBucket"/> to compare to.</param>
        /// <returns>Whether the <see cref="CommandCooldownBucket"/> is equal to this <see cref="CommandCooldownBucket"/>.</returns>
        public bool Equals(CommandCooldownBucket other)
        {
            if (ReferenceEquals(other, null))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return UserId == other.UserId && ChannelId == other.ChannelId && GuildId == other.GuildId;
        }

        /// <summary>
        /// Gets the hash code for this <see cref="CommandCooldownBucket"/>.
        /// </summary>
        /// <returns>The hash code for this <see cref="CommandCooldownBucket"/>.</returns>
        public override int GetHashCode()
        {
            int hash = 13;

            hash = hash * 7 + UserId.GetHashCode();
            hash = hash * 7 + ChannelId.GetHashCode();
            hash = hash * 7 + GuildId.GetHashCode();

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
            var null1 = bucket1 is null;
            var null2 = bucket2 is null;

            if (null1 && null2)
            {
                return true;
            }

            if (null1 != null2)
            {
                return false;
            }

            return false;
        }

        /// <summary>
        /// Gets whether the two <see cref="CommandCooldownBucket"/> objects are not equal.
        /// </summary>
        /// <param name="bucket1">First bucket to compare.</param>
        /// <param name="bucket2">Second bucket to compare.</param>
        /// <returns>Whether the two buckets are not equal.</returns>
        public static bool operator !=(CommandCooldownBucket bucket1, CommandCooldownBucket bucket2) =>
            !(bucket1 == bucket2);

        /// <summary>
        /// Creates a bucket ID from given bucket parameters.
        /// </summary>
        /// <param name="user">ID of the user with which this cooldown is associated.</param>
        /// <param name="channel">ID of the channel with which this cooldown is associated.</param>
        /// <param name="guild">ID of the guild with which this cooldown is associated.</param>
        /// <returns>Generated bucket ID.</returns>
        public static string MakeId(ulong user = 0, ulong channel = 0, ulong guild = 0) =>
            $"{user.ToString(CultureInfo.InvariantCulture)}:{channel.ToString(CultureInfo.InvariantCulture)}:{guild.ToString(CultureInfo.InvariantCulture)}";
    }
}
