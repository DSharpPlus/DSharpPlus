using System;
using System.Collections.Generic;
using DSharpPlus.Core.Enums;
using Newtonsoft.Json;

namespace DSharpPlus.Core.RestEntities
{
    public sealed record DiscordSelectMenuComponent : IDiscordMessageComponent
    {
        /// <inheritdoc/>
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordComponentType Type { get; init; }

        /// <summary>
        /// A developer-defined identifier for the select menu, max 100 characters.
        /// </summary>
        [JsonProperty("custom_id", NullValueHandling = NullValueHandling.Ignore)]
        public string CustomId { get; init; } = null!;

        /// <summary>
        /// The choices in the select, max 25.
        /// </summary>
        [JsonProperty("options", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<DiscordSelectMenuOptionComponent> Options { get; init; } = Array.Empty<DiscordSelectMenuOptionComponent>();

        /// <summary>
        /// The custom placeholder text if nothing is selected, max 150 characters.
        /// </summary>
        [JsonProperty("placeholder", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<string> Placeholder { get; init; }

        /// <summary>
        /// The minimum number of items that must be chosen; default 1, min 0, max 25.
        /// </summary>
        [JsonProperty("min_values", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<int> MinValues { get; init; }

        /// <summary>
        /// The maximum number of items that can be chosen; default 1, max 25.
        /// </summary>
        [JsonProperty("max_values", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<int> MaxValues { get; init; }

        /// <summary>
        /// Whether to disable the select, default false.
        /// </summary>
        [JsonProperty("disabled", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<bool> Disabled { get; init; }
    }
}
