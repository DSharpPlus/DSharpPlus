using DSharpPlus.Core.Enums;
using Newtonsoft.Json;

namespace DSharpPlus.Core.RestEntities
{
    public sealed record DiscordTextInputComponent : IDiscordMessageComponent
    {
        /// <inheritdoc/>
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordComponentType Type { get; init; }

        /// <summary>
        /// A developer-defined identifier for the input, max 100 characters.
        /// </summary>
        [JsonProperty("custom_id", NullValueHandling = NullValueHandling.Ignore)]
        public string CustomId { get; init; } = null!;

        /// <summary>
        /// The <see cref="DiscordTextInputStyle"/> Text Input Style</see>.
        /// </summary>
        [JsonProperty("style", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordTextInputStyle Style { get; init; }

        /// <summary>
        /// The label for this component, max 45 characters.
        /// </summary>
        [JsonProperty("label", NullValueHandling = NullValueHandling.Ignore)]
        public string Label { get; init; } = null!;

        /// <summary>
        /// The minimum input length for a text input, min 0, max 4000.
        /// </summary>
        [JsonProperty("min_length", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<int> MinLength { get; init; }

        /// <summary>
        /// The maximum input length for a text input, min 1, max 4000.
        /// </summary>
        [JsonProperty("max_length", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<int> MaxLength { get; init; }

        /// <summary>
        /// Whether this component is required to be filled, default true.
        /// </summary>
        [JsonProperty("required", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<bool> Required { get; init; }

        /// <summary>
        /// A pre-filled value for this component, max 4000 characters.
        /// </summary>
        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<string> Value { get; init; }

        /// <summary>
        /// A custom placeholder text if the input is empty, max 100 characters.
        /// </summary>
        [JsonProperty("placeholder", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<string> Placeholder { get; init; }
    }
}
