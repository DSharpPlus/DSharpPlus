using System.Text.Json.Serialization;

namespace DSharpPlus.Core.Entities
{
    public sealed record DiscordGuildScheduledEventEntityMetadata
    {
        /// <summary>
        /// Location of the event (1-100 characters).
        /// </summary>
        /// <remarks>
        /// Requires <see cref="DiscordGuildScheduledEvent.EntityType"/> to be <see cref="Enums.DiscordGuildScheduledEventEntityType.External"/>.
        /// </remarks>
        [JsonPropertyName("location")]
        public string? Location { get; init; }
    }
}
