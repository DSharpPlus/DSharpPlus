using DSharpPlus.Core.Enums;
using System.Text.Json.Serialization;

namespace DSharpPlus.Core.Entities
{
    public sealed record InternalTextInputComponent : IInternalMessageComponent
    {
        /// <inheritdoc/>
        [JsonPropertyName("type")]
        public InternalComponentType Type { get; init; }

        /// <summary>
        /// A developer-defined identifier for the input, max 100 characters.
        /// </summary>
        [JsonPropertyName("custom_id")]
        public string CustomId { get; init; } = null!;

        /// <summary>
        /// The <see cref="InternalTextInputStyle"/> Text Input Style</see>.
        /// </summary>
        [JsonPropertyName("style")]
        public InternalTextInputStyle Style { get; init; }

        /// <summary>
        /// The label for this component, max 45 characters.
        /// </summary>
        [JsonPropertyName("label")]
        public string Label { get; init; } = null!;

        /// <summary>
        /// The minimum input length for a text input, min 0, max 4000.
        /// </summary>
        [JsonPropertyName("min_length")]
        public Optional<int> MinLength { get; init; }

        /// <summary>
        /// The maximum input length for a text input, min 1, max 4000.
        /// </summary>
        [JsonPropertyName("max_length")]
        public Optional<int> MaxLength { get; init; }

        /// <summary>
        /// Whether this component is required to be filled, default true.
        /// </summary>
        [JsonPropertyName("required")]
        public Optional<bool> Required { get; init; }

        /// <summary>
        /// A pre-filled value for this component, max 4000 characters.
        /// </summary>
        [JsonPropertyName("value")]
        public Optional<string> Value { get; init; }

        /// <summary>
        /// A custom placeholder text if the input is empty, max 100 characters.
        /// </summary>
        [JsonPropertyName("placeholder")]
        public Optional<string> Placeholder { get; init; }
    }
}
