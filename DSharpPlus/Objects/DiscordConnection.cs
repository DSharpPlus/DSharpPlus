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
<<<<<<< HEAD
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
=======
        [JsonProperty("name")]
>>>>>>> master
        public string Name { get; internal set; }
        /// <summary>
        /// The service of the connection
        /// </summary>
<<<<<<< HEAD
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
=======
        [JsonProperty("type")]
>>>>>>> master
        public string Type { get; internal set; }
        /// <summary>
        /// Whether the connection is revoked
        /// </summary>
<<<<<<< HEAD
        [JsonProperty("revoked", NullValueHandling = NullValueHandling.Ignore)]
=======
        [JsonProperty("revoked")]
>>>>>>> master
        public bool Revoked { get; internal set; }
        /// <summary>
        /// A list of partial server integrations
        /// </summary>
<<<<<<< HEAD
        [JsonProperty("integrations", NullValueHandling = NullValueHandling.Ignore)]
=======
        [JsonProperty("integrations")]
>>>>>>> master
        public List<DiscordIntegration> Integrations { get; internal set; }
    }
}
