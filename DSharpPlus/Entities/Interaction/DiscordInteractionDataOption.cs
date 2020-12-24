using System.Collections.Generic;
using Newtonsoft.Json;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents parameters for interaction commands. 
    /// </summary>
    public class DiscordInteractionDataOption
    {
        /// <summary>
        /// Gets the name of this interaction parameter.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; internal set; }

        /// <summary>
        /// Gets the option type of this interaction parameter.
        /// </summary>
        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        public ApplicationCommandOptionType OptionType { get; internal set; }

        /// <summary>
        /// Gets the additional parameters if this parameter is a subcommand.
        /// </summary>
        [JsonProperty("options", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<DiscordInteractionDataOption> Options { get; internal set; }
    }
}
