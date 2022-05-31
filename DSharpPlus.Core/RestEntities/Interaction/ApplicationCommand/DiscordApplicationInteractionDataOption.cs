using System.Collections.Generic;
using DSharpPlus.Core.Enums;
using Newtonsoft.Json;

namespace DSharpPlus.Core.RestEntities
{
    /// <summary>
    /// All options have names, and an option can either be a parameter and input value--in which case value will be set--or it can denote a subcommand or group--in which case it will contain a top-level key and another array of options.
    /// </summary>
    public sealed record DiscordApplicationInteractionDataOption
    {
        /// <summary>
        /// The name of the parameter.
        /// </summary>
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; init; } = null!;

        /// <summary>
        /// The value of <see cref="DiscordApplicationCommandOptionType"/>.
        /// </summary>
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordApplicationCommandOptionType Type { get; init; }

        /// <summary>
        /// The value of the option resulting from user input.
        /// </summary>
        /// <remarks>
        /// A string, integer, or double. Mutually exclusive with <see cref="Options"/>.
        /// </remarks>
        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<object> Value { get; init; }

        /// <summary>
        /// Only present if this option is a <see cref="DiscordApplicationCommandOptionType.SubCommand"/> or <see cref="DiscordApplicationCommandOptionType.SubCommandGroup"/>.
        /// </summary>
        /// <remarks>
        /// Mutually exclusive with <see cref="Value"/>.
        /// </remarks>
        [JsonProperty("options", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<IReadOnlyList<DiscordApplicationInteractionDataOption>> Options { get; init; }

        /// <summary>
        /// True if this option is the currently focused option for autocomplete.
        /// </summary>
        [JsonProperty("focused", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<bool> Focused { get; init; }
    }
}
