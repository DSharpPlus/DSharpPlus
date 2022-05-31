using System.Collections.Generic;
using DSharpPlus.Core.Attributes;
using DSharpPlus.Core.Enums;
using Newtonsoft.Json;

namespace DSharpPlus.Core.RestEntities
{
    [DiscordGatewayPayload("INTERACTION_CREATE")]
    public sealed record DiscordInteraction
    {
        /// <summary>
        /// The ID of the invoked command.
        /// </summary>
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake Id { get; init; } = null!;

        /// <summary>
        /// The name of the invoked command.
        /// </summary>
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; init; } = null!;

        /// <summary>
        /// The type of the invoked command.
        /// </summary>
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordInteractionType Type { get; init; }

        /// <summary>
        /// The converted users + roles + channels + attachments attached to the interaction.
        /// </summary>
        [JsonProperty("resolved", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordInteractionResolvedData> Resolved { get; init; }

        /// <summary>
        /// The params + values from the user.
        /// </summary>
        [JsonProperty("options", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<IReadOnlyList<DiscordApplicationInteractionDataOption>> Options { get; init; }

        /// <summary>
        /// The id of the guild the command is registered to.
        /// </summary>
        [JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordSnowflake> GuildId { get; init; }

        /// <summary>
        /// The custom_id of the component.
        /// </summary>
        [JsonProperty("custom_id", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<string> CustomId { get; init; }

        /// <summary>
        /// The type of the component.
        /// </summary>
        [JsonProperty("component_type", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordComponentType> ComponentType { get; init; }

        /// <summary>
        /// The values the user selected.
        /// </summary>
        [JsonProperty("values", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<IReadOnlyList<DiscordSelectOptionValue>> Values { get; init; }

        /// <summary>
        /// The id the of user or message targeted by a user or message command.
        /// </summary>
        [JsonProperty("target_id", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordSnowflake> TargetId { get; init; }

        /// <summary>
        /// The values submitted by the user.
        /// </summary>
        [JsonProperty("components", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<IReadOnlyList<IDiscordMessageComponent>> Components { get; init; }
    }
}
