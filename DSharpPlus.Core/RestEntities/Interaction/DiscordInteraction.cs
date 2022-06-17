using System.Collections.Generic;
using DSharpPlus.Core.Attributes;
using DSharpPlus.Core.Enums;
using System.Text.Json.Serialization;

namespace DSharpPlus.Core.RestEntities
{
    [DiscordGatewayPayload("INTERACTION_CREATE")]
    public sealed record DiscordInteraction
    {
        /// <summary>
        /// The ID of the invoked command.
        /// </summary>
        [JsonPropertyName("id")]
        public DiscordSnowflake Id { get; init; } = null!;

        /// <summary>
        /// The name of the invoked command.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; init; } = null!;

        /// <summary>
        /// The type of the invoked command.
        /// </summary>
        [JsonPropertyName("type")]
        public DiscordInteractionType Type { get; init; }

        /// <summary>
        /// The converted users + roles + channels + attachments attached to the interaction.
        /// </summary>
        [JsonPropertyName("resolved")]
        public Optional<DiscordInteractionResolvedData> Resolved { get; init; }

        /// <summary>
        /// The params + values from the user.
        /// </summary>
        [JsonPropertyName("options")]
        public Optional<IReadOnlyList<DiscordApplicationInteractionDataOption>> Options { get; init; }

        /// <summary>
        /// The id of the guild the command is registered to.
        /// </summary>
        [JsonPropertyName("guild_id")]
        public Optional<DiscordSnowflake> GuildId { get; init; }

        /// <summary>
        /// The custom_id of the component.
        /// </summary>
        [JsonPropertyName("custom_id")]
        public Optional<string> CustomId { get; init; }

        /// <summary>
        /// The type of the component.
        /// </summary>
        [JsonPropertyName("component_type")]
        public Optional<DiscordComponentType> ComponentType { get; init; }

        /// <summary>
        /// The values the user selected.
        /// </summary>
        [JsonPropertyName("values")]
        public Optional<IReadOnlyList<DiscordSelectOptionValue>> Values { get; init; }

        /// <summary>
        /// The id the of user or message targeted by a user or message command.
        /// </summary>
        [JsonPropertyName("target_id")]
        public Optional<DiscordSnowflake> TargetId { get; init; }

        /// <summary>
        /// The values submitted by the user.
        /// </summary>
        [JsonPropertyName("components")]
        public Optional<IReadOnlyList<IDiscordMessageComponent>> Components { get; init; }
    }
}
