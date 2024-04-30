namespace DSharpPlus.VoiceNext;
using Microsoft.Extensions.Logging;

/// <summary>
/// Contains well-defined event IDs used by the VoiceNext extension.
/// </summary>
public static class VoiceNextEvents
{
    /// <summary>
    /// Miscellaneous events, that do not fit in any other category.
    /// </summary>
    public static EventId Misc { get; } = new EventId(300, "VoiceNext");

    /// <summary>
    /// Events pertaining to Voice Gateway connection lifespan, specifically, heartbeats.
    /// </summary>
    public static EventId VoiceHeartbeat { get; } = new EventId(301, nameof(VoiceHeartbeat));

    /// <summary>
    /// Events pertaining to Voice Gateway connection early lifespan, specifically, the establishing thereof as well as negotiating various modes.
    /// </summary>
    public static EventId VoiceHandshake { get; } = new EventId(302, nameof(VoiceHandshake));

    /// <summary>
    /// Events emitted when incoming voice data is corrupted, or packets are being dropped.
    /// </summary>
    public static EventId VoiceReceiveFailure { get; } = new EventId(303, nameof(VoiceReceiveFailure));

    /// <summary>
    /// Events pertaining to UDP connection lifespan, specifically the keepalive (or heartbeats).
    /// </summary>
    public static EventId VoiceKeepalive { get; } = new EventId(304, nameof(VoiceKeepalive));

    /// <summary>
    /// Events emitted for high-level dispatch receive events.
    /// </summary>
    public static EventId VoiceDispatch { get; } = new EventId(305, nameof(VoiceDispatch));

    /// <summary>
    /// Events emitted for Voice Gateway connection closes, clean or otherwise.
    /// </summary>
    public static EventId VoiceConnectionClose { get; } = new EventId(306, nameof(VoiceConnectionClose));

    /// <summary>
    /// Events emitted when decoding data received via Voice Gateway fails for any reason.
    /// </summary>
    public static EventId VoiceGatewayError { get; } = new EventId(307, nameof(VoiceGatewayError));

    /// <summary>
    /// Events containing raw (but decompressed) payloads, received from Discord Voice Gateway.
    /// </summary>
    public static EventId VoiceWsRx { get; } = new EventId(308, "Voice ↓");

    /// <summary>
    /// Events containing raw payloads, as they're being sent to Discord Voice Gateway.
    /// </summary>
    public static EventId VoiceWsTx { get; } = new EventId(309, "Voice ↑");
}
