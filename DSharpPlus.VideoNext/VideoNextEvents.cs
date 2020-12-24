using Microsoft.Extensions.Logging;

namespace DSharpPlus.VideoNext
{
    /// <summary>
    /// Contains well-defined event IDs used by the VoiceNext extension.
    /// </summary>
    public static class VideoNextEvents
    {
        /// <summary>
        /// Miscellaneous events, that do not fit in any other category.
        /// </summary>
        public static EventId Misc { get; } = new EventId(300, "VideoNext");
        
        /// <summary>
        /// Events pertaining to Video Gateway connection lifespan, specifically, heartbeats.
        /// </summary>
        public static EventId VideoHeartbeat { get; } = new EventId(301, nameof(VideoHeartbeat));
        
        /// <summary>
        /// Events pertaining to Video Gateway connection early lifespan, specifically, the establishing thereof as well as negotiating various modes.
        /// </summary>
        public static EventId VideoHandshake { get; } = new EventId(302, nameof(VideoHandshake));

        /// <summary>
        /// Events emitted when incoming voice data is corrupted, or packets are being dropped.
        /// </summary>
        public static EventId VideoReceiveFailure { get; } = new EventId(303, nameof(VideoReceiveFailure));
        
        /// <summary>
        /// Events pertaining to UDP connection lifespan for video, specifically the keepalive (or heartbeats).
        /// </summary>
        public static EventId VideoKeepalive { get; } = new EventId(304, nameof(VideoKeepalive));
        

        /// <summary>
        /// Events emitted for high-level dispatch receive events.
        /// </summary>
        public static EventId VideoDispatch { get; } = new EventId(305, nameof(VideoDispatch));
        

        /// <summary>
        /// Events emitted for Video Gateway connection closes, clean or otherwise.
        /// </summary>
        public static EventId VideoConnectionClose { get; } = new EventId(306, nameof(VideoConnectionClose));


        /// <summary>
        /// Events emitted when decoding data received via Voice Gateway fails for any reason.
        /// </summary>
        public static EventId VideoGatewayError { get; } = new EventId(307, nameof(VideoGatewayError));
        
        
        /// <summary>
        /// Events containing raw (but decompressed) payloads, received from Discord Video Gateway.
        /// </summary>
        public static EventId VideoWsRx { get; } = new EventId(308, "Video ↓");
        
        
        /// <summary>
        /// Events containing raw payloads, as they're being sent to Discord Voice Gateway.
        /// </summary>
        public static EventId VideoWsTx { get; } = new EventId(309, "Video ↑");
    }
}
