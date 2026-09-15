using System;

namespace DSharpPlus.Net.Gateway;

/// <summary>
/// Controls the behaviour of the default <see cref="IGatewayClient"/>.
/// </summary>
public sealed class GatewayClientOptions
{
    /// <summary>
    /// Specifies a function to get the reconnection delay on a given consecutive attempt to reconnect.
    /// </summary>
    /// <remarks>
    /// Defaults to doubling the time spent waiting until 2^10 seconds, or 17:04 minutes are reached, at which point
    /// the value becomes constant.
    /// </remarks>
    public Func<uint, TimeSpan> GetReconnectionDelay { get; set; }
        = (num) => TimeSpan.FromSeconds(double.Pow(2, uint.Min(num, 10)));

    /// <summary>
    /// Specifies a timeout for how long we are willing to wait for the HELLO event. If HELLO is not received within 
    /// that timespan, the shard is forced to reconnect. Defaults to 15 seconds.
    /// </summary>
    public TimeSpan HelloEventTimeout { get; set; } = TimeSpan.FromSeconds(15);

    /// <summary>
    /// Specifies the maximum amount of reconnects to attempt consecutively. The counter resets if a connection is
    /// successfully established. Defaults to <see cref="uint.MaxValue"/>.
    /// </summary>
    public uint MaxReconnects { get; set; } = uint.MaxValue;

    /// <summary>
    /// Specifies the member count at which guilds are considered "large" and the information sent about members is
    /// reduced. Defaults to 250.
    /// </summary>
    public int LargeThreshold { get; set; } = 250;

    /// <summary>
    /// Specifies the gateway intents for this client. The client will only receive events they specified the relevant
    /// intents for. Defaults to <see cref="DiscordIntents.AllUnprivileged"/>.
    /// </summary>
    public DiscordIntents Intents { get; set; } = DiscordIntents.AllUnprivileged;

    /// <summary>
    /// Specifies the timeout to use when sending a payload to the gateway and for websocket-level keepalive messages
    /// (this is distinct from heartbeating). If this is exceeded, DSharpPlus will attempt to reconnect and resume. 
    /// Defaults to 5 seconds.
    /// </summary>
    /// <remarks>
    /// This is intended to detect transient connection failures where we need to reconnect, but will also come into
    /// effect during Discord outages and if your internet connection is slow or prone to spikes in latency.
    /// </remarks>
    public TimeSpan SendingTimeout { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Specifies the amount of heartbeats to let go unanswered before assuming the connection zombied and restarting.
    /// </summary>
    /// <remarks>
    /// Defaults to 5. The exact timespan this represents is contingent on the heartbeat interval for the connection and
    /// not predictable.
    /// </remarks>
    public uint ZombiedThreshold { get; set; } = 5;

    /// <summary>
    /// Specifies the amount of heartbeats to tolerate sending before receiving READY. Once this amount is exceeded,
    /// DSharpPlus assumes something has gone wrong and reconnects
    /// </summary>
    public uint HeartbeatsBeforeReadyThreshold { get; set; } = 5;

    /// <summary>
    /// Specifies a delay to use when retrying sending a gateway message. This is only invoked if WriteAsync was called
    /// during a resume procedure and would have failed as a result of the otherwise transparent resumption. Defaults to
    /// 250ms.
    /// </summary>
    public TimeSpan WriteRetryDelay { get; set; } = TimeSpan.FromMilliseconds(250);

    /// <summary>
    /// Specifies the amount of times a write may be reattempted. This is only invoked if WriteAsync was called during a
    /// resume procedure and would have failed as a result of the otherwise transparent resumption. Defaults to 5 attempts.
    /// </summary>
    public int WriteRetryAttempts { get; set; } = 5;
}
