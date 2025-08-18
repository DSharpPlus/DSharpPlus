using System;

using DSharpPlus.Net.Abstractions;

using Newtonsoft.Json;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Fired if a request made over the gateway got ratelimited.
/// </summary>
public sealed class RatelimitedEventArgs : DiscordEventArgs
{
    /// <summary>
    /// The opcode of the request that got ratelimited.
    /// </summary>
    [JsonProperty("opcode")]
    public GatewayOpCode Opcode { get; private set; }

    /// <summary>
    /// Indicates how long we need to wait to retry.
    /// </summary>
    [JsonIgnore]
    public TimeSpan RetryAfter => TimeSpan.FromSeconds(this.retryAfter);

    [JsonProperty("retry_after")]
#pragma warning disable IDE0044 // Add readonly modifier - set by newtonsoft.json
    private float retryAfter = 30.0f;
#pragma warning restore IDE0044

    /// <summary>
    /// Additional information about the request that got ratelimited. The exact type depends on <see cref="Opcode"/>: <br/>
    /// - <see cref="GatewayOpCode.RequestGuildMembers"/> corresponds to <see cref="RequestGuildMembersRatelimitMetadata"/>.
    /// </summary>
    [JsonIgnore]
    public RatelimitMetadata Metadata { get; internal set; }
}
