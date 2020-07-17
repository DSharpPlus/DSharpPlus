using Microsoft.Extensions.Logging;

namespace DSharpPlus.VoiceNext
{
    internal static class VoiceNextEvents
    {
        public static EventId Misc { get; } = new EventId(300, "VoiceNext");
        public static EventId VoiceHeartbeat { get; } = new EventId(301, nameof(VoiceHeartbeat));
        public static EventId VoiceHandshake { get; } = new EventId(302, nameof(VoiceHandshake));
        public static EventId VoiceReceiveFailure { get; } = new EventId(303, nameof(VoiceReceiveFailure));
        public static EventId VoiceKeepalive { get; } = new EventId(304, nameof(VoiceKeepalive));
        public static EventId VoiceDispatch { get; } = new EventId(305, nameof(VoiceDispatch));
        public static EventId VoiceConnectionClose { get; } = new EventId(306, nameof(VoiceConnectionClose));
        public static EventId VoiceGatewayError { get; } = new EventId(307, nameof(VoiceGatewayError));
    }
}
