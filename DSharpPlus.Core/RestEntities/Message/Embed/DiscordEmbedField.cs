using Newtonsoft.Json;

namespace DSharpPlus.Core.RestEntities
{
    public sealed record DiscordEmbedField
    {
        /// <summary>
        /// The name of the field.
        /// </summary>
        /// <remarks>
        /// Max 256 characters.
        /// </remarks>
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; init; } = null!;

        /// <summary>
        /// The value of the field.
        /// </summary>
        /// <remarks>
        /// Max 1024 characters.
        /// </remarks>
        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        public string Value { get; init; } = null!;

        /// <summary>
        /// Whether or not this field should display inline.
        /// </summary>
        [JsonProperty("inline", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<bool> Inline { get; init; }
    }
}
