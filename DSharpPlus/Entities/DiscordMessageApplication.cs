using Newtonsoft.Json;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents a Rich Presence application.
    /// </summary>
    public class DiscordMessageApplication : SnowflakeObject
    {
        /// <summary>
        /// Gets the ID of this application's cover image.
        /// </summary>
        [JsonProperty("cover_image")]
        public string CoverImageId { get; internal set; }

        /// <summary>
        /// Gets the description of this application.
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; internal set; }

        /// <summary>
        /// Gets the ID of the application's icon.
        /// </summary>
        [JsonProperty("icon")]
        public string IconId { get; internal set; }

        /// <summary>
        /// Gets the name of this application.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; internal set; }

        internal DiscordMessageApplication() { }

        
    }
}
