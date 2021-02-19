using Newtonsoft.Json;
using System.Collections.Generic;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents a parameter for a <see cref="DiscordApplicationCommand"/>.
    /// </summary>
    public sealed class DiscordApplicationCommandOption
    {
        /// <summary>
        /// Gets the type of this command parameter.
        /// </summary>
        [JsonProperty("type")]
        public ApplicationCommandOptionType Type { get; internal set; }

        /// <summary>
        /// Gets the name of this command parameter.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; internal set; }

        /// <summary>
        /// Gets the description of this command parameter.
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; internal set; }

        /// <summary>
        /// Gets whether this command parameter is required.
        /// </summary>
        [JsonProperty("required", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Required { get; internal set; }

        /// <summary>
        /// Gets the optional choices for this command parameter.
        /// </summary>
        [JsonProperty("choices", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyCollection<DiscordApplicationCommandOptionChoice> Choices { get; internal set; }

        /// <summary>
        /// Gets the optional subcommand parameters for this parameter.
        /// </summary>
        [JsonProperty("options", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyCollection<DiscordApplicationCommandOption> Options { get; internal set; }
    }
}
