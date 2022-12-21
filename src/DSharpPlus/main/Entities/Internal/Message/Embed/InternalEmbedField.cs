using System.Text.Json.Serialization;

namespace DSharpPlus.Core.Entities
{
    public sealed record InternalEmbedField
    {
        /// <summary>
        /// The name of the field.
        /// </summary>
        /// <remarks>
        /// Max 256 characters.
        /// </remarks>
        [JsonPropertyName("name")]
        public string Name { get; init; } = null!;

        /// <summary>
        /// The value of the field.
        /// </summary>
        /// <remarks>
        /// Max 1024 characters.
        /// </remarks>
        [JsonPropertyName("value")]
        public string Value { get; init; } = null!;

        /// <summary>
        /// Whether or not this field should display inline.
        /// </summary>
        [JsonPropertyName("inline")]
        public Optional<bool> Inline { get; init; }
    }
}
