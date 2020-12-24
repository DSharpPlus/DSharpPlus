using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents the inner data payload of an interaction.
    /// </summary>
    public class DiscordInteractionData : SnowflakeObject
    {
        /// <summary>
        /// Gets the name of the invoked interaction.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; internal set; }

        /// <summary>
        /// Gets the parameters and values of the invoked interaction.
        /// </summary>
        [JsonProperty("options", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<DiscordInteractionDataOption> Options { get; internal set; }
    }
}
