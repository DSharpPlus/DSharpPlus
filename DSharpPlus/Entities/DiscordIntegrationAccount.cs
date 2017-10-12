using Newtonsoft.Json;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents a Discord integration account.
    /// </summary>
    public class DiscordIntegrationAccount : SnowflakeObject
    {
        /// <summary>
        /// Gets the name of the account.
        /// </summary>
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; internal set; }

        internal DiscordIntegrationAccount() { }
    }
}
