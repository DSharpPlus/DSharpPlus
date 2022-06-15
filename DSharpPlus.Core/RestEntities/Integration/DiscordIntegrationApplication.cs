using Newtonsoft.Json;

namespace DSharpPlus.Core.RestEntities
{
    public sealed record DiscordIntegrationApplication
    {
        /// <summary>
        /// The id of the app.
        /// </summary>
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake Id { get; init; } = null!;

        /// <summary>
        /// The name of the app.
        /// </summary>
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; init; } = null!;

        /// <summary>
        /// The icon hash of the app.
        /// </summary>
        [JsonProperty("icon", NullValueHandling = NullValueHandling.Ignore)]
        public string? Icon { get; init; }

        /// <summary>
        /// The description of the app.
        /// </summary>
        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; init; } = null!;

        /// <summary>
        /// The bot associated with this application.
        /// </summary>
        [JsonProperty("bot", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordUser> Bot { get; init; }
    }
}
