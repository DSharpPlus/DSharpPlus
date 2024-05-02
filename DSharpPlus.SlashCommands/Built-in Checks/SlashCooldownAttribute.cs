using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace DSharpPlus.SlashCommands.Attributes;

/// <summary>
/// Defines a cooldown for this command. This allows you to define how many times can users execute a specific command
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class SlashCooldownAttribute : SlashCheckBaseAttribute
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
    public SlashCooldownBucketType BucketType { get; }

    /// <summary>
    /// Gets the cooldown buckets for this command.
    /// </summary>
    private static readonly ConcurrentDictionary<string, SlashCommandCooldownBucket> _buckets = new();

    /// <summary>
    /// Defines a cooldown for this command. This means that users will be able to use the command a specific number of times before they have to wait to use it again.
    /// </summary>
    /// <param name="maxUses">Number of times the command can be used before triggering a cooldown.</param>
    /// <param name="resetAfter">Number of seconds after which the cooldown is reset.</param>
    /// <param name="bucketType">Type of cooldown bucket. This allows controlling whether the bucket will be cooled down per user, guild, channel, or globally.</param>
    public SlashCooldownAttribute(int maxUses, double resetAfter, SlashCooldownBucketType bucketType)
    {
        MaxUses = maxUses;
        Reset = TimeSpan.FromSeconds(resetAfter);
        BucketType = bucketType;
    }

    /// <summary>
    /// Gets a cooldown bucket for given command context.
    /// </summary>
    /// <param name="ctx">Command context to get cooldown bucket for.</param>
    /// <returns>Requested cooldown bucket, or null if one wasn't present.</returns>
    public SlashCommandCooldownBucket GetBucket(InteractionContext ctx)
    {
        string bid = GetBucketId(ctx, out _, out _, out _);
        _buckets.TryGetValue(bid, out SlashCommandCooldownBucket? bucket);
        return bucket;
    }

    /// <summary>
    /// Calculates the cooldown remaining for given command context.
    /// </summary>
    /// <param name="ctx">Context for which to calculate the cooldown.</param>
    /// <returns>Remaining cooldown, or zero if no cooldown is active.</returns>
    public TimeSpan GetRemainingCooldown(InteractionContext ctx)
    {
        SlashCommandCooldownBucket? bucket = GetBucket(ctx);
        return (bucket is null || bucket.RemainingUses > 0) ? TimeSpan.Zero : bucket.ResetsAt - DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Calculates bucket ID for given command context.
    /// </summary>
    /// <param name="ctx">Context for which to calculate bucket ID for.</param>
    /// <param name="userId">ID of the user with which this bucket is associated.</param>
    /// <param name="channelId">ID of the channel with which this bucket is associated.</param>
    /// <param name="guildId">ID of the guild with which this bucket is associated.</param>
    /// <returns>Calculated bucket ID.</returns>
    private string GetBucketId(InteractionContext ctx, out ulong userId, out ulong channelId, out ulong guildId)
    {
        userId = 0ul;
        if (BucketType.HasFlag(SlashCooldownBucketType.User))
        {
            userId = ctx.User.Id;
        }

        channelId = 0ul;
        if (BucketType.HasFlag(SlashCooldownBucketType.Channel))
        {
            channelId = ctx.Channel.Id;
        }

        guildId = 0ul;
        if (BucketType.HasFlag(SlashCooldownBucketType.Guild))
        {
            if (ctx.Guild == null)
            {
                channelId = ctx.Channel.Id;
            }
            else
            {
                guildId = ctx.Guild.Id;
            }
        }

        string bucketId = SlashCommandCooldownBucket.MakeId(ctx.QualifiedName, ctx.Client.CurrentUser.Id, userId, channelId, guildId);
        return bucketId;
    }

    public override async Task<bool> ExecuteChecksAsync(InteractionContext ctx)
    {
        string bucketId = GetBucketId(ctx, out ulong userId, out ulong channelId, out ulong guildId);
        if (!_buckets.TryGetValue(bucketId, out SlashCommandCooldownBucket? bucket))
        {
            bucket = new SlashCommandCooldownBucket(ctx.QualifiedName, ctx.Client.CurrentUser.Id, MaxUses, Reset, userId, channelId, guildId);
            _buckets.AddOrUpdate(bucketId, bucket, (key, value) => bucket);
        }

        return await bucket.DecrementUseAsync();
    }
}

/// <summary>
/// Defines how are command cooldowns applied.
/// </summary>
public enum SlashCooldownBucketType : int
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
public sealed class SlashCommandCooldownBucket : IEquatable<SlashCommandCooldownBucket>
{
    /// <summary>
    /// The command's full name (includes groups and subcommands).
    /// </summary>
    public string FullCommandName { get; }

    /// <summary>
    /// The bot's ID.
    /// </summary>
    public ulong BotId { get; }

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
    private SemaphoreSlim _usageSemaphore { get; }

    /// <summary>
    /// Creates a new command cooldown bucket.
    /// </summary>
    /// <param name="fullCommandName">Full name of the command.</param>
    /// <param name="botId">ID of the bot.</param>
    /// <param name="maxUses">Maximum number of uses for this bucket.</param>
    /// <param name="resetAfter">Time after which this bucket resets.</param>
    /// <param name="userId">ID of the user with which this cooldown is associated.</param>
    /// <param name="channelId">ID of the channel with which this cooldown is associated.</param>
    /// <param name="guildId">ID of the guild with which this cooldown is associated.</param>
    internal SlashCommandCooldownBucket(string fullCommandName, ulong botId, int maxUses, TimeSpan resetAfter, ulong userId = 0, ulong channelId = 0, ulong guildId = 0)
    {
        FullCommandName = fullCommandName;
        BotId = botId;
        MaxUses = maxUses;
        ResetsAt = DateTimeOffset.UtcNow + resetAfter;
        Reset = resetAfter;
        UserId = userId;
        ChannelId = channelId;
        GuildId = guildId;
        BucketId = MakeId(fullCommandName, botId, userId, channelId, guildId);
        _remainingUses = maxUses;
        _usageSemaphore = new SemaphoreSlim(1, 1);
    }

    /// <summary>
    /// Decrements the remaining use counter.
    /// </summary>
    /// <returns>Whether decrement succeeded or not.</returns>
    internal async Task<bool> DecrementUseAsync()
    {
        await _usageSemaphore.WaitAsync();

        // if we're past reset time...
        DateTimeOffset now = DateTimeOffset.UtcNow;
        if (now >= ResetsAt)
        {
            // ...do the reset and set a new reset time
            Interlocked.Exchange(ref _remainingUses, MaxUses);
            ResetsAt = now + Reset;
        }

        // check if we have any uses left, if we do...
        bool success = false;
        if (RemainingUses > 0)
        {
            // ...decrement, and return success...
            Interlocked.Decrement(ref _remainingUses);
            success = true;
        }

        // ...otherwise just fail
        _usageSemaphore.Release();
        return success;
    }

    /// <summary>
    /// Returns a string representation of this command cooldown bucket.
    /// </summary>
    /// <returns>String representation of this command cooldown bucket.</returns>
    public override string ToString() => $"Command bucket {BucketId}";

    /// <summary>
    /// Checks whether this <see cref="SlashCommandCooldownBucket"/> is equal to another object.
    /// </summary>
    /// <param name="obj">Object to compare to.</param>
    /// <returns>Whether the object is equal to this <see cref="SlashCommandCooldownBucket"/>.</returns>
    public override bool Equals(object obj) => obj is SlashCommandCooldownBucket cooldownBucket && Equals(cooldownBucket);

    /// <summary>
    /// Checks whether this <see cref="SlashCommandCooldownBucket"/> is equal to another <see cref="SlashCommandCooldownBucket"/>.
    /// </summary>
    /// <param name="other"><see cref="SlashCommandCooldownBucket"/> to compare to.</param>
    /// <returns>Whether the <see cref="SlashCommandCooldownBucket"/> is equal to this <see cref="SlashCommandCooldownBucket"/>.</returns>
    public bool Equals(SlashCommandCooldownBucket other) => other is not null && (ReferenceEquals(this, other) || (UserId == other.UserId && ChannelId == other.ChannelId && GuildId == other.GuildId));

    /// <summary>
    /// Gets the hash code for this <see cref="SlashCommandCooldownBucket"/>.
    /// </summary>
    /// <returns>The hash code for this <see cref="SlashCommandCooldownBucket"/>.</returns>
    public override int GetHashCode() => HashCode.Combine(UserId, ChannelId, GuildId);

    /// <summary>
    /// Gets whether the two <see cref="SlashCommandCooldownBucket"/> objects are equal.
    /// </summary>
    /// <param name="bucket1">First bucket to compare.</param>
    /// <param name="bucket2">Second bucket to compare.</param>
    /// <returns>Whether the two buckets are equal.</returns>
    public static bool operator ==(SlashCommandCooldownBucket bucket1, SlashCommandCooldownBucket bucket2)
    {
        bool null1 = bucket1 is null;
        bool null2 = bucket2 is null;

        return (null1 && null2) || (null1 == null2 && null1.Equals(null2));
    }

    /// <summary>
    /// Gets whether the two <see cref="SlashCommandCooldownBucket"/> objects are not equal.
    /// </summary>
    /// <param name="bucket1">First bucket to compare.</param>
    /// <param name="bucket2">Second bucket to compare.</param>
    /// <returns>Whether the two buckets are not equal.</returns>
    public static bool operator !=(SlashCommandCooldownBucket bucket1, SlashCommandCooldownBucket bucket2) => !(bucket1 == bucket2);

    /// <summary>
    /// Creates a bucket ID from given bucket parameters.
    /// </summary>
    /// <param name="fullCommandName">Full name of the command with which this cooldown is associated.</param>
    /// <param name="botId">ID of the bot with which this cooldown is associated.</param>
    /// <param name="userId">ID of the user with which this cooldown is associated.</param>
    /// <param name="channelId">ID of the channel with which this cooldown is associated.</param>
    /// <param name="guildId">ID of the guild with which this cooldown is associated.</param>
    /// <returns>Generated bucket ID.</returns>
    public static string MakeId(string fullCommandName, ulong botId, ulong userId = 0, ulong channelId = 0, ulong guildId = 0)
        => $"{userId.ToString(CultureInfo.InvariantCulture)}:{channelId.ToString(CultureInfo.InvariantCulture)}:{guildId.ToString(CultureInfo.InvariantCulture)}:{botId}:{fullCommandName}";
}
