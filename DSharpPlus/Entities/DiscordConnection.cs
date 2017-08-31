using System.Collections.Generic;
using Newtonsoft.Json;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Gets a Discord connection to a 3rd party service.
    /// </summary>
    public class DiscordConnection : SnowflakeObject
    {
        /// <summary>
        /// Gets the username of the connected account.
        /// </summary>
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; internal set; }

        /// <summary>
        /// Gets the name of the connection service.
        /// </summary>
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; internal set; }

        /// <summary>
        /// Gets whether the connection is revoked.
        /// </summary>
        [JsonProperty("revoked", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsRevoked { get; internal set; }

        /// <summary>
        /// Gets a collection of partial server integrations.
        /// </summary>
        [JsonProperty("integrations", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<DiscordIntegration> Integrations { get; internal set; }
    }
}
