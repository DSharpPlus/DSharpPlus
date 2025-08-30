using System.Text.Json.Serialization;

namespace DSharpPlus.Voice.Models;
/// <summary>
/// Represents the inner data used when selecting the protocol
/// for a Discord voice connection. This information is provided
/// by the client to the voice server during setup.
/// </summary>
public class VoiceSelectProtocolInnerData
{
    /// <summary>
    /// Gets or sets the external IP address of the client
    /// that will be used for the voice connection.
    /// </summary>
    [JsonPropertyName("address")]
    public string Address { get; set; }

    /// <summary>
    /// Gets or sets the UDP port of the client that will be used
    /// for the voice connection.
    /// </summary>
    [JsonPropertyName("port")]
    public int Port { get; set; }

    /// <summary>
    /// Gets or sets the encryption mode (e.g., <c>xsalsa20_poly1305</c>)
    /// to be used for securing the voice data stream.
    /// </summary>
    [JsonPropertyName("mode")]
    public string Mode { get; set; }
}
