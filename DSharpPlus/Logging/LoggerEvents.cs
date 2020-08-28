using Microsoft.Extensions.Logging;

namespace DSharpPlus
{
    internal static class LoggerEvents
    {
        public static EventId Misc { get; } = new EventId(100, "DSharpPlus");
        public static EventId Startup { get; } = new EventId(101, nameof(Startup));
        public static EventId ConnectionFailure { get; } = new EventId(102, nameof(ConnectionFailure));
        public static EventId SessionUpdate { get; } = new EventId(103, nameof(SessionUpdate));
        public static EventId EventHandlerException { get; } = new EventId(104, nameof(EventHandlerException));
        public static EventId WebSocketReceive { get; } = new EventId(105, nameof(WebSocketReceive));
        public static EventId WebSocketReceiveRaw { get; } = new EventId(106, nameof(WebSocketReceiveRaw));
        public static EventId WebSocketSendRaw { get; } = new EventId(107, nameof(WebSocketSendRaw));
        public static EventId WebSocketReceiveFailure { get; } = new EventId(108, nameof(WebSocketReceiveFailure));
        public static EventId Heartbeat { get; } = new EventId(109, nameof(Heartbeat));
        public static EventId HeartbeatFailure { get; } = new EventId(110, nameof(HeartbeatFailure));
        public static EventId ConnectionClose { get; } = new EventId(111, nameof(ConnectionClose));
        public static EventId RestError { get; } = new EventId(112, nameof(RestError));
        public static EventId ShardStartup { get; } = new EventId(113, nameof(ShardStartup));
        public static EventId RatelimitHit { get; } = new EventId(114, nameof(RatelimitHit));
        public static EventId RatelimitDiag { get; } = new EventId(115, nameof(RatelimitDiag));
        public static EventId RatelimitPreemptive { get; } = new EventId(116, nameof(RatelimitPreemptive));
        public static EventId AuditLog { get; } = new EventId(117, nameof(AuditLog));
        public static EventId GatewayWsRx { get; } = new EventId(118, "Gateway ↓");
        public static EventId GatewayWsTx { get; } = new EventId(119, "Gateway ↑");
        public static EventId Intents { get; } = new EventId(120, nameof(Intents));
    }
}
