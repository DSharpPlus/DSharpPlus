using Newtonsoft.Json;

namespace DSharpPlus.Core.RestEntities
{
    public sealed record DiscordSelectMenuOptionComponent
    {
        /// <summary>
        /// The user-facing name of the option, max 100 characters.
        /// </summary>
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Label { get; init; } = null!;

        /// <summary>
        /// The dev-defined value of the option, max 100 characters.
        /// </summary>
        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        public string Value { get; init; } = null!;

        /// <summary>
        /// An additional description of the option, max 100 characters.
        /// </summary>
        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<string> Description { get; init; }

        /// <summary>
        /// The id, name, and animated properties.
        /// </summary>
        [JsonProperty("emoji", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordEmoji> Emoji { get; init; }

        /// <summary>
        /// Whether to render this option as selected by default.
        /// </summary>
        [JsonProperty("default", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<bool> Default { get; init; }
    }
}
