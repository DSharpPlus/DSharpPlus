using System.Text.Json.Serialization;

namespace DSharpPlus.Voice.Transport.Models.VoicePayloads.Inbound;

/// <summary>
/// Represents the data included in a voice heartbeat acknowledgment
/// payload. This event is sent by the Discord voice server in
/// response to a client heartbeat, confirming receipt.
/// </summary>
public class VoiceHeartbeatAckData
{
    /// <summary>
    /// Gets or sets the server timestamp (in milliseconds since the epoch)
    /// when the heartbeat was acknowledged.
    /// </summary>
    [JsonPropertyName("t")]
    public long Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the sequence number acknowledged by the server,
    /// corresponding to the last heartbeat sent by the client.
    /// </summary>
    [JsonPropertyName("seq_ack")]
    public ushort SequenceAck { get; set; }
}
