using System.Text.Json.Serialization;

namespace DSharpPlus.Voice.Protocol.Gateway.DaveV1.Outbound;

internal sealed record VoiceHeartbeatData
{
    /// <summary>
    /// The timestamp at the time of sending, used as an integer nonce.
    /// </summary>
    [JsonPropertyName("t")]
    public required ulong Timestamp { get; init; }

    /// <summary>
    /// The last received sequence number from the voice gateway.
    /// </summary>
    [JsonPropertyName("seq_ack")]
    public required int LastSequence { get; init; }
}