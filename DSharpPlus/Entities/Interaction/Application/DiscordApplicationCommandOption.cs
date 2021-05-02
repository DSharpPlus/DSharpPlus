using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json;

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

        /// <summary>
        /// Creates a new instance of a <see cref="DiscordApplicationCommandOption"/>.
        /// </summary>
        /// <param name="name">The name of this parameter.</param>
        /// <param name="description">The description of the parameter.</param>
        /// <param name="type">The type of this parameter.</param>
        /// <param name="required">Whether the parameter is required.</param>
        /// <param name="choices">The optional choice selection for this parameter.</param>
        /// <param name="options">The optional subcommands for this parameter.</param>
        public DiscordApplicationCommandOption(string name, string description, ApplicationCommandOptionType type, bool? required = null, IEnumerable<DiscordApplicationCommandOptionChoice> choices = null, IEnumerable<DiscordApplicationCommandOption> options = null)
        {
            var choiceList = choices != null ? new ReadOnlyCollection<DiscordApplicationCommandOptionChoice>(choices.ToList()) : null;
            var optionList = options != null ? new ReadOnlyCollection<DiscordApplicationCommandOption>(options.ToList()) : null;

            this.Name = name;
            this.Description = description;
            this.Type = type;
            this.Required = required;
            this.Choices = choiceList;
            this.Options = optionList;
        }
    }
}
