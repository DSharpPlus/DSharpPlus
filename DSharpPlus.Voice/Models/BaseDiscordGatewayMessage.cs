using System.Text.Json.Serialization;

/// <summary>
/// Represents the base structure of a Discord Gateway message.
/// Provides common fields shared across most Gateway events,
/// including the operation code and event type.
/// </summary>
public class BaseDiscordGatewayMessage
{
    /// <summary>
    /// Gets or sets the operation code (<c>op</c>) that indicates
    /// the type of Gateway event or instruction.
    /// </summary>
    public int OpCode { get; set; }

    /// <summary>
    /// Gets or sets the event type (<c>t</c>) as a string.
    /// For example, this may be <c>"READY"</c>, <c>"MESSAGE_CREATE"</c>,
    /// or other Gateway event identifiers.
    /// </summary>
    [JsonPropertyName("t")]
    public string Type { get; set; }
}
