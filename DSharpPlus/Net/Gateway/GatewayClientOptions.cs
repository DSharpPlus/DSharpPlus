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
    /// Specifies the maximum amount of reconnects to attempt consecutively. The counter resets if a connection is
    /// successfully established. Defaults to <see cref="uint.MaxValue"/>.
    /// </summary>
    public uint MaxReconnects { get; set; } = uint.MaxValue;

    /// <summary>
    /// Specifies whether the gateway should attempt to reconnect automatically, if possible. It will always attempt
    /// to resume a session, regardless of this setting. Defaults to true.
    /// </summary>
    public bool AutoReconnect { get; set; } = true;

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
}
