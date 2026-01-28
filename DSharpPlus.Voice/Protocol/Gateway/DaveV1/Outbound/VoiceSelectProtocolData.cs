using System.Text.Json.Serialization;

namespace DSharpPlus.Voice.Protocol.Gateway.DaveV1.Outbound;

/// <summary>
/// Represents the data included in a voice "select protocol" payload.
/// This is sent by the client to the Discord voice server to specify
/// which transport protocol and configuration should be used for
/// the voice connection.
/// </summary>
public class VoiceSelectProtocolData
{
    /// <summary>
    /// Gets or sets the transport protocol to be used for the voice
    /// connection. This is typically <c>"udp"</c>.
    /// </summary>
    [JsonPropertyName("protocol")]
    public string Protocol { get; set; } = "udp";

    /// <summary>
    /// Gets or sets the inner data object that contains protocol-specific
    /// configuration details such as IP address, port, and encryption mode.
    /// </summary>
    [JsonPropertyName("data")]
    public VoiceSelectProtocolInnerData InnerData { get; set; }
}
