using System.Text.Json.Serialization;

namespace DSharpPlus.Entities.Internal;

public sealed record InternalGuildScheduledEventEntityMetadata
{
    /// <summary>
    /// Location of the event (1-100 characters).
    /// </summary>
    /// <remarks>
    /// Requires <see cref="InternalGuildScheduledEvent.EntityType"/> to be <see cref="Enums.InternalGuildScheduledEventEntityType.External"/>.
    /// </remarks>
    [JsonPropertyName("location")]
    public string? Location { get; init; }
}
