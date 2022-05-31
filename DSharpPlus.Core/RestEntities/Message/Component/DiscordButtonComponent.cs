using DSharpPlus.Core.Enums;
using Newtonsoft.Json;

namespace DSharpPlus.Core.RestEntities
{
    public sealed record DiscordButtonComponent : IDiscordMessageComponent
    {
        /// <inheritdoc/>
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordComponentType Type { get; init; }

        /// <summary>
        /// One of the <see cref="DiscordButtonStyle">button styles</see>.
        /// </summary>
        /// <remarks>
        /// Non-link buttons must have a <see cref="CustomId"/>, and cannot have a url. Link buttons must have a url, and cannot have a <see cref="CustomId"/>. Link buttons do not send an <see cref="DiscordInteraction"/> to your app when clicked.
        /// </remarks>
        [JsonProperty("style", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordButtonStyle Style { get; init; }

        /// <summary>
        /// Text that appears on the button, max 80 characters.
        /// </summary>
        [JsonProperty("label", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<string> Label { get; init; }

        /// <summary>
        /// The name, id, and animated properties.
        /// </summary>
        [JsonProperty("emoji", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordEmoji> Emoji { get; init; }

        /// <summary>
        /// A developer-defined identifier for the button, max 100 characters.
        /// </summary>
        /// <remarks>
        /// <see cref="CustomId"/> must be unique per component; multiple buttons on the same message must not share the same <see cref="CustomId"/>. This field is a string of max 100 characters, and can be used flexibly to maintain state or pass through other important data.
        /// </remarks>
        [JsonProperty("custom_id", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<string> CustomId { get; init; }

        /// <summary>
        /// A url for <see cref="DiscordButtonStyle.Link"/> buttons.
        /// </summary>
        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<string> Url { get; init; }

        /// <summary>
        /// Whether the button is disabled (default <see langword="false"/>).
        /// </summary>
        [JsonProperty("disabled", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<bool> Disabled { get; init; }
    }
}
