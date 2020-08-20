using Microsoft.Extensions.Logging;

namespace DSharpPlus.Lavalink
{
    internal static class LavalinkEvents
    {
        public static EventId Misc { get; } = new EventId(400, "Lavalink");
        public static EventId LavalinkConnectionError { get; } = new EventId(401, nameof(LavalinkConnectionError));
        public static EventId LavalinkConnectionClosed { get; } = new EventId(402, nameof(LavalinkConnectionClosed));
        public static EventId LavalinkConnected { get; } = new EventId(403, nameof(LavalinkConnected));
        public static EventId LavalinkDecodeError { get; } = new EventId(404, nameof(LavalinkDecodeError));
        public static EventId LavalinkRestError { get; } = new EventId(405, nameof(LavalinkRestError));
        public static EventId LavalinkWsRx { get; } = new EventId(406, "Lavalink ↓");
        public static EventId LavalinkWsTx { get; } = new EventId(407, "Lavalink ↑");
    }
}
