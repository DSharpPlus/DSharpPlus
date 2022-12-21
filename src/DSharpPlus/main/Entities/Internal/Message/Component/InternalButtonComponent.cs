using DSharpPlus.Core.Enums;
using System.Text.Json.Serialization;

namespace DSharpPlus.Core.Entities
{
    public sealed record InternalButtonComponent : IInternalMessageComponent
    {
        /// <inheritdoc/>
        [JsonPropertyName("type")]
        public InternalComponentType Type { get; init; }

        /// <summary>
        /// One of the <see cref="InternalButtonStyle">button styles</see>.
        /// </summary>
        /// <remarks>
        /// Non-link buttons must have a <see cref="CustomId"/>, and cannot have a url. Link buttons must have a url, and cannot have a <see cref="CustomId"/>. Link buttons do not send an <see cref="InternalInteraction"/> to your app when clicked.
        /// </remarks>
        [JsonPropertyName("style")]
        public InternalButtonStyle Style { get; init; }

        /// <summary>
        /// Text that appears on the button, max 80 characters.
        /// </summary>
        [JsonPropertyName("label")]
        public Optional<string> Label { get; init; }

        /// <summary>
        /// The name, id, and animated properties.
        /// </summary>
        [JsonPropertyName("emoji")]
        public Optional<InternalEmoji> Emoji { get; init; }

        /// <summary>
        /// A developer-defined identifier for the button, max 100 characters.
        /// </summary>
        /// <remarks>
        /// <see cref="CustomId"/> must be unique per component; multiple buttons on the same message must not share the same <see cref="CustomId"/>. This field is a string of max 100 characters, and can be used flexibly to maintain state or pass through other important data.
        /// </remarks>
        [JsonPropertyName("custom_id")]
        public Optional<string> CustomId { get; init; }

        /// <summary>
        /// A url for <see cref="InternalButtonStyle.Link"/> buttons.
        /// </summary>
        [JsonPropertyName("url")]
        public Optional<string> Url { get; init; }

        /// <summary>
        /// Whether the button is disabled (default <see langword="false"/>).
        /// </summary>
        [JsonPropertyName("disabled")]
        public Optional<bool> Disabled { get; init; }
    }
}
