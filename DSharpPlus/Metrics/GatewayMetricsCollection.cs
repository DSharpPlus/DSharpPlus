using System;

namespace DSharpPlus.Metrics;

/// <summary>
/// Represents an immutable snapshot of gateway metrics.
/// </summary>
public readonly record struct GatewayMetricsCollection
{
    /// <summary>
    /// The duration covered by these metrics.
    /// </summary>
    public TimeSpan Duration { get; init; }

    /// <summary>
    /// The amount of shards managed by DSharpPlus while these metrics were collected.
    /// </summary>
    public int ShardCount { get; init; }

    /// <summary>
    /// The amount of events received from the gateway.
    /// </summary>
    public ulong EventsReceived { get; init; }

    /// <summary>
    /// The amount of bytes received from the gateway. If compression was enabled, this is the compressed size.
    /// </summary>
    public ulong BytesReceived { get; init; }

    /// <summary>
    /// The amount of decompressed bytes received.
    /// </summary>
    public ulong DecompressedBytesReceived { get; init; }

    /// <summary>
    /// The amount of events sent to the gateway.
    /// </summary>
    public ulong EventsSent { get; init; }

    /// <summary>
    /// The amount of bytes sent to the gateway.
    /// </summary>
    public ulong BytesSent { get; init; }

    /// <summary>
    /// The amount of resume attempts made in total.
    /// </summary>
    public int ResumeAttempts => this.SuccessfulResumes + this.FailedResumes;

    /// <summary>
    /// The amount of successful session resume procedures executed.
    /// </summary>
    public int SuccessfulResumes { get; init; }

    /// <summary>
    /// The amount of times resuming a session failed.
    /// </summary>
    public int FailedResumes { get; init; }

    /// <summary>
    /// The amount of hard reconnects executed.
    /// </summary>
    public int Reconnects { get; init; }
}
