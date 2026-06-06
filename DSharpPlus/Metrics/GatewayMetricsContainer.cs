using System;
using System.Threading;
using DSharpPlus.Clients;

namespace DSharpPlus.Metrics;

/// <summary>
/// Provides a mechanism to obtain metrics about the gateway connections managed by DSharpPlus.
/// </summary>
public sealed class GatewayMetricsContainer
{
    private LiveGatewayMetrics metrics;
    private readonly DateTimeOffset creation = DateTimeOffset.UtcNow;
    private readonly int shardCount;

    public GatewayMetricsContainer(IShardOrchestrator orchestrator)
        => this.shardCount = orchestrator.ConnectedShardCount;

    /// <summary>
    /// Gets a snapshot of metrics as collected so far.
    /// </summary>
    public GatewayMetricsCollection GetCollectedMetrics()
    {
        return new()
        {
            Duration = DateTimeOffset.UtcNow - this.creation,
            ShardCount = this.shardCount,

            EventsReceived = this.metrics.eventsReceived,
            BytesReceived = this.metrics.bytesReceived,
            EventsSent = this.metrics.eventsSent,
            BytesSent = this.metrics.bytesSent,
            Reconnects = this.metrics.reconnects
        };
    }

    internal void RecordGatewayEventSent(int bytes)
    {
        Interlocked.Increment(ref this.metrics.eventsSent);
        Interlocked.Add(ref this.metrics.bytesSent, (ulong)bytes);
        GlobalMeter.RecordGatewayEventSent(bytes);
    }

    internal void RecordGatewayEventReceived(int bytes)
    {
        Interlocked.Increment(ref this.metrics.eventsReceived);
        Interlocked.Add(ref this.metrics.bytesReceived, (ulong)bytes);
        GlobalMeter.RecordGatewayEventReceived(bytes);
    }

    internal void RecordGatewayEventDecompressed(int bytes)
    {
        Interlocked.Add(ref this.metrics.decompressedBytesReceived, (ulong)bytes);
        GlobalMeter.RecordGatewayEventDecompressed(bytes);
    }

    internal void RecordReconnect()
    {
        Interlocked.Increment(ref this.metrics.reconnects);
        GlobalMeter.RecordReconnect();
    }

    private struct LiveGatewayMetrics
    {
        public ulong eventsReceived;
        public ulong bytesReceived;
        public ulong decompressedBytesReceived;
        public ulong eventsSent;
        public ulong bytesSent;
        public int reconnects;
    }
}
