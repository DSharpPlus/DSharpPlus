using Newtonsoft.Json;

namespace DSharpPlus.Core.RestEntities
{
    public sealed record DiscordActivityButton
    {
        /// <summary>
        /// The text shown on the button (1-32 characters).
        /// </summary>
        [JsonProperty("label", NullValueHandling = NullValueHandling.Ignore)]
        public string Label { get; init; } = null!;

        /// <summary>
        /// The url opened when clicking the button (1-512 characters).
        /// </summary>
        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public string Url { get; init; } = null!;
    }
}
