namespace DSharpPlus.Net;

using System;
using Newtonsoft.Json;

/// <summary>
/// Represents the bucket limits for identifying to Discord.
/// <para>This is only relevant for clients that are manually sharding.</para>
/// </summary>
public class SessionBucket
{
    /// <summary>
    /// Gets the total amount of sessions per token.
    /// </summary>
    [JsonProperty("total")]
    public int Total { get; internal set; }

    /// <summary>
    /// Gets the remaining amount of sessions for this token.
    /// </summary>
    [JsonProperty("remaining")]
    public int Remaining { get; internal set; }

    /// <summary>
    /// Gets the datetime when the <see cref="Remaining"/> will reset.
    /// </summary>
    [JsonIgnore]
    public DateTimeOffset ResetAfter { get; internal set; }

    /// <summary>
    /// Gets the maximum amount of shards that can boot concurrently.
    /// </summary>
    [JsonProperty("max_concurrency")]
    public int MaxConcurrency { get; internal set; }

    [JsonProperty("reset_after")]
    internal int ResetAfterInternal { get; set; }

    public override string ToString()
        => $"[{Remaining}/{Total}] {ResetAfter}. {MaxConcurrency}x concurrency";
}
