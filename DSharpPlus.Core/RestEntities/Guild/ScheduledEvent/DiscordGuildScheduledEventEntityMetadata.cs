using Newtonsoft.Json;

namespace DSharpPlus.Core.RestEntities
{
    public sealed record DiscordGuildScheduledEventEntityMetadata
    {
        /// <summary>
        /// Location of the event (1-100 characters).
        /// </summary>
        /// <remarks>
        /// Requires <see cref="DiscordGuildScheduledEvent.EntityType"/> to be <see cref="Enums.DiscordGuildScheduledEventEntityType.External"/>.
        /// </remarks>
        [JsonProperty("location", NullValueHandling = NullValueHandling.Ignore)]
        public string? Location { get; init; }
    }
}
