
using Newtonsoft.Json;

namespace DSharpPlus.Net;
/// <summary>
/// Represents information used to identify with Discord.
/// </summary>
public sealed class GatewayInfo
{
    /// <summary>
    /// Gets the gateway URL for the WebSocket connection.
    /// </summary>
    [JsonProperty("url")]
    public string Url { get; set; }

    /// <summary>
    /// Gets the recommended amount of shards.
    /// </summary>
    [JsonProperty("shards")]
    public int ShardCount { get; internal set; }

    /// <summary>
    /// Gets the session start limit data.
    /// </summary>
    [JsonProperty("session_start_limit")]
    public SessionBucket SessionBucket { get; internal set; }
}
