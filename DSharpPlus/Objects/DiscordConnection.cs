using System.Collections.Generic;
using Newtonsoft.Json;

namespace DSharpPlus.Objects
{
    /// <summary>
    ///
    /// </summary>
    public class DiscordConnection : SnowflakeObject
    {
        /// <summary>
        /// The username of the connection account
        /// </summary>
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; internal set; }
        /// <summary>
        /// The service of the connection
        /// </summary>
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; internal set; }
        /// <summary>
        /// Whether the connection is revoked
        /// </summary>
        [JsonProperty("revoked", NullValueHandling = NullValueHandling.Ignore)]
        public bool Revoked { get; internal set; }
        /// <summary>
        /// A list of partial server integrations
        /// </summary>
        [JsonProperty("integrations", NullValueHandling = NullValueHandling.Ignore)]
        public List<DiscordIntegration> Integrations { get; internal set; }
    }
}
