using Newtonsoft.Json;
using System.Collections.Generic;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents a field in a guild's membership screening form
    /// </summary>
    public class DiscordGuildMembershipScreeningField
    {
        /// <summary>
        /// Gets the type of the field. Currently defualts to TERMS.
        /// </summary>
        [JsonProperty("field_type")]
        public string Type { get; internal set; }

        /// <summary>
        /// Gets the title of the field.
        /// </summary>
        [JsonProperty("label")]
        public string Label { get; internal set; }

        /// <summary>
        /// Gets the list of rules
        /// </summary>
        [JsonProperty("values")]
        public IReadOnlyList<string> Values { get; internal set; }

        /// <summary>
        /// Gets whether the user has to fill out this field
        /// </summary>
        [JsonProperty("required")]
        public bool IsRequired { get; internal set; }

        internal DiscordGuildMembershipScreeningField() { }
    }
}
