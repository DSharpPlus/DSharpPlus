using System;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Represents arguments for Heartbeated event.
/// </summary>
[Obsolete("This event is obsolete and wont be invoked. Use IGatewayController.HeartbeatedAsync instead")]
public class HeartbeatedEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Gets the round-trip time of the heartbeat.
    /// </summary>
    public int Ping { get; internal set; }

    /// <summary>
    /// Gets the timestamp of the heartbeat.
    /// </summary>
    public DateTimeOffset Timestamp { get; internal set; }

    internal HeartbeatedEventArgs() : base() { }
}
