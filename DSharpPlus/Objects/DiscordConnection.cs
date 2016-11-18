using Newtonsoft.Json;
using System.Collections.Generic;

namespace DSharpPlus
{
    /// <summary>
    ///
    /// </summary>
    public class DiscordConnection : SnowflakeObject
    {
        /// <summary>
        /// The username of the connection account
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; internal set; }
        /// <summary>
        /// The service of the connection
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; internal set; }
        /// <summary>
        /// Whether the connection is revoked
        /// </summary>
        [JsonProperty("revoked")]
        public bool Revoked { get; internal set; }
        /// <summary>
        /// A list of partial server integrations
        /// </summary>
        [JsonProperty("integrations")]
        public List<DiscordIntegration> Integrations { get; internal set; }
    }
}
