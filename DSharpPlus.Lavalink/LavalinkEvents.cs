namespace DSharpPlus.Lavalink;
using Microsoft.Extensions.Logging;

/// <summary>
/// Contains well-defined event IDs used by the Lavalink extension.
/// </summary>
public static class LavalinkEvents
{
    /// <summary>
    /// Miscellaneous events, that do not fit in any other category.
    /// </summary>
    public static EventId Misc { get; } = new EventId(400, "Lavalink");

    /// <summary>
    /// Events pertaining to Lavalink node connection errors.
    /// </summary>
    public static EventId LavalinkConnectionError { get; } = new EventId(401, nameof(LavalinkConnectionError));

    /// <summary>
    /// Events emitted for clean disconnects from Lavalink.
    /// </summary>
    public static EventId LavalinkConnectionClosed { get; } = new EventId(402, nameof(LavalinkConnectionClosed));

    /// <summary>
    /// Events emitted for successful connections made to Lavalink.
    /// </summary>
    public static EventId LavalinkConnected { get; } = new EventId(403, nameof(LavalinkConnected));

    /// <summary>
    /// Events pertaining to errors that occur when decoding payloads received from Lavalink nodes.
    /// </summary>
    public static EventId LavalinkDecodeError { get; } = new EventId(404, nameof(LavalinkDecodeError));

    /// <summary>
    /// Events emitted when Lavalink's REST API responds with an error.
    /// </summary>
    public static EventId LavalinkRestError { get; } = new EventId(405, nameof(LavalinkRestError));

    /// <summary>
    /// Events containing raw payloads, received from Lavalink nodes.
    /// </summary>
    public static EventId LavalinkWsRx { get; } = new EventId(406, "Lavalink ↓");

    /// <summary>
    /// Events containing raw payloads, as they're being sent to Lavalink nodes.
    /// </summary>
    public static EventId LavalinkWsTx { get; } = new EventId(407, "Lavalink ↑");

    /// <summary>
    /// Events pertaining to Gateway Intents. Typically diagnostic information.
    /// </summary>
    public static EventId Intents { get; } = new EventId(408, nameof(Intents));
}
