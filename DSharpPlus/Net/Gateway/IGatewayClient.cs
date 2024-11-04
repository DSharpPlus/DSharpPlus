using System;
using System.Threading.Tasks;

using DSharpPlus.Entities;
using DSharpPlus.Net.Abstractions;

namespace DSharpPlus.Net.Gateway;

/// <summary>
/// Represents a gateway client handling all system events.
/// </summary>
public interface IGatewayClient
{
    /// <summary>
    /// Connects this client to the gateway.
    /// </summary>
    /// <param name="url">The gateway URL to use for connecting, including any potential reconnects.</param>
    /// <param name="activity">An optional activity to send to the gateway when connecting.</param>
    /// <param name="status">An optional status to send to the gateway when connecting.</param>
    /// <param name="idleSince">An optional idle timer to send to the gateway when connecting.</param>
    /// <param name="shardInfo">If this isn't the only shard, additional information about this shard.</param>
    public ValueTask ConnectAsync
    (
        string url,
        DiscordActivity? activity = null,
        DiscordUserStatus? status = null,
        DateTimeOffset? idleSince = null,
        ShardInfo? shardInfo = null
    );

    /// <summary>
    /// Disconnects from the gateway.
    /// </summary>
    public ValueTask DisconnectAsync();

    /// <summary>
    /// Reconnects to the gateway.
    /// </summary>
    public ValueTask ReconnectAsync();

    /// <summary>
    /// Sends the provided payload to the gateway.
    /// </summary>
    public ValueTask WriteAsync(byte[] payload);

    /// <summary>
    /// Indicates whether this client is connected.
    /// </summary>
    public bool IsConnected { get; }

    /// <summary>
    /// Indicates the latency between this client and Discord.
    /// </summary>
    public TimeSpan Ping { get; }

    /// <summary>
    /// Gets the shard ID of this client. Defaults to zero if not sharding.
    /// </summary>
    public int ShardId { get; }
}
