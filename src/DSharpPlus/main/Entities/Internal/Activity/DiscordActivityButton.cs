using System.Text.Json.Serialization;

namespace DSharpPlus.Core.Entities
{
    public sealed record DiscordActivityButton
    {
        /// <summary>
        /// The text shown on the button (1-32 characters).
        /// </summary>
        [JsonPropertyName("label")]
        public string Label { get; init; } = null!;

        /// <summary>
        /// The url opened when clicking the button (1-512 characters).
        /// </summary>
        [JsonPropertyName("url")]
        public string Url { get; init; } = null!;
    }
}
