namespace DSharpPlus.Clients;

/// <summary>
/// Contains configuration options for sharding within DSharpPlus.
/// </summary>
public sealed class ShardingOptions
{
    /// <summary>
    /// Specifies the amount of shards to start. If left default, the library will decide automatically.
    /// </summary>
    public uint? ShardCount { get; set; }

    /// <summary>
    /// Specifies the amount of shards IDs to skip when starting up, designed for multi-process sharding.
    /// </summary>
    public uint Stride { get; set; }

    /// <summary>
    /// Specifies the amount of total shards associated with this bot. This is only considered in combination with
    /// <see cref="Stride"/>.
    /// </summary>
    public uint TotalShards { get; set; }
}
