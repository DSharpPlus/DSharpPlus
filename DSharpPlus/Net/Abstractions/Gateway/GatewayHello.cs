namespace DSharpPlus.Net.Abstractions;
using System.Collections.Generic;
using Newtonsoft.Json;

/// <summary>
/// Represents data for a websocket hello payload.
/// </summary>
internal sealed class GatewayHello
{
    /// <summary>
    /// Gets the target heartbeat interval (in milliseconds) requested by Discord.
    /// </summary>
    [JsonProperty("heartbeat_interval")]
    public int HeartbeatInterval { get; private set; }

    /// <summary>
    /// Gets debug data sent by Discord. This contains a list of servers to which the client is connected.
    /// </summary>
    [JsonProperty("_trace")]
    public IReadOnlyList<string> Trace { get; private set; }
}
