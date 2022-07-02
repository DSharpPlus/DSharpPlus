using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DSharpPlus.Core.Entities
{
    public sealed record DiscordInteractionResolvedData
    {
        /// <summary>
        /// The ids and User objects.
        /// </summary>
        [JsonPropertyName("users")]
        public Optional<IReadOnlyDictionary<DiscordSnowflake, DiscordUser>> Users { get; init; }

        /// <summary>
        /// The ids and partial Member objects.
        /// </summary>
        [JsonPropertyName("members")]
        public Optional<IReadOnlyDictionary<DiscordSnowflake, DiscordGuildMember>> Members { get; init; }

        /// <summary>
        /// The ids and Role objects.
        /// </summary>
        [JsonPropertyName("roles")]
        public Optional<IReadOnlyDictionary<DiscordSnowflake, DiscordRole>> Roles { get; init; }

        /// <summary>
        /// The ids and partial Channel objects.
        /// </summary>
        [JsonPropertyName("channels")]
        public Optional<IReadOnlyDictionary<DiscordSnowflake, DiscordChannel>> Channels { get; init; }

        /// <summary>
        /// The ids and partial Message objects.
        /// </summary>
        [JsonPropertyName("messages")]
        public Optional<IReadOnlyDictionary<DiscordSnowflake, DiscordMessage>> Messages { get; init; }

        /// <summary>
        /// The ids and attachment objects.
        /// </summary>
        [JsonPropertyName("attachments")]
        public Optional<IReadOnlyDictionary<DiscordSnowflake, DiscordAttachment>> Attachments { get; init; }
    }
}
