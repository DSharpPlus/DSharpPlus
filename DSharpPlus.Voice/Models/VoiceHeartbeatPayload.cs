using System.Text.Json.Serialization;

namespace DSharpPlus.Voice.Models;

/// <summary>
/// Represents a heartbeat payload in the Discord voice protocol.
/// This payload is periodically sent by the client to the voice server
/// to maintain an active connection and prevent timeouts.
/// </summary>
public class VoiceHeartbeatPayload
{
    /// <summary>
    /// Gets or sets the operation code (<c>op</c>) that specifies
    /// the type of voice event or instruction.
    /// </summary>
    [JsonPropertyName("op")]
    public int OpCode { get; set; }

    /// <summary>
    /// Gets or sets the sequence number (<c>d</c>) that should be
    /// included in the heartbeat. This is used by the server to
    /// track connection state and ensure messages are received in order.
    /// </summary>
    [JsonPropertyName("d")]
    public ushort Sequence { get; set; }
}
