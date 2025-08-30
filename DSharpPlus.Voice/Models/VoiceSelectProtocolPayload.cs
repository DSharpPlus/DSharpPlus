using System.Text.Json.Serialization;

namespace DSharpPlus.Voice.Models;
/// <summary>
/// Represents a "select protocol" payload in the Discord voice protocol.
/// This payload is sent by the client to the voice server to finalize
/// which transport protocol (usually UDP) and configuration will be
/// used for the voice connection.
/// </summary>
public class VoiceSelectProtocolPayload
{
    /// <summary>
    /// Gets or sets the operation code (<c>op</c>).
    /// For "select protocol," this is always <c>1</c>.
    /// </summary>
    [JsonPropertyName("op")]
    public int OpCode { get; set; } = 1; // Always 1 for Select Protocol

    /// <summary>
    /// Gets or sets the data object (<c>d</c>) containing the protocol
    /// type and connection details such as address, port, and mode.
    /// </summary>
    [JsonPropertyName("d")]
    public VoiceSelectProtocolData Data { get; set; }
}
