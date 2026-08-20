using System;

namespace DSharpPlus.Net.Gateway;

/// <summary>
/// Represents the outcome of running a gateway connection until it couldn't recover.
/// </summary>
public sealed class GatewayConnectionFrame
{
    /// <summary>
    /// Specifies why the gateway connection closed and returned to the orchestrator.
    /// </summary>
    public required GatewayDisconnectReason DisconnectReason { get; init; }

    /// <summary>
    /// Gets the exception, if one occurred.
    /// </summary>
    public Exception? Exception { get; init; }

    /// <summary>
    /// Gets the close code received from Discord, if one occurred.
    /// </summary>
    public GatewayCloseCode? CloseCode { get; init; }

    /// <summary>
    /// Gets the ID of the shard that terminated.
    /// </summary>
    public int ShardId { get; init; }
}
