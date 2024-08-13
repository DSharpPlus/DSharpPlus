using Newtonsoft.Json;

namespace DSharpPlus.Net.Abstractions;

/// <summary>
/// Internal implementation detail to communicate shard IDs between IGatewayClient and dispatch.
/// </summary>
internal sealed class ShardIdContainingGatewayPayload : GatewayPayload
{
    /// <summary>
    /// Gets the shard ID that invoked this event.
    /// </summary>
    [JsonIgnore]
    public int ShardId { get; internal set; }
}
