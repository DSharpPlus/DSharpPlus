using System.Text.Json.Serialization;

namespace DSharpPlus.Voice.Protocol.Gateway.DaveV1;
public class DiscordGatewayMessage<PayloadT>
{
    /// <summary>
    /// Gets/Sets the type
    /// </summary>
    [JsonPropertyName("t")]
    public string? Type { get; set; }

    /// <summary>
    /// Gets or sets the sequence number (<c>seq</c>) associated with
    /// this payload. Used to maintain message ordering.
    /// </summary>
    [JsonPropertyName("seq")]
    public int? Sequence { get; set; }

    /// <summary>
    /// Gets or sets the operation code (<c>op</c>) that specifies
    /// the type of voice event or instruction.
    /// </summary>
    [JsonPropertyName("op")]
    public int OpCode { get; set; }

    /// <summary>
    /// Gets or sets the data object (<c>d</c>) containing details
    /// about the epoch preparation, such as protocol version and epoch ID.
    /// </summary>
    [JsonPropertyName("d")]
    public PayloadT Data { get; set; }
}
