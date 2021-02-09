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
        [JsonProperty("field_type", NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; internal set; }

        /// <summary>
        /// Gets the title of the field.
        /// </summary>
        [JsonProperty("label", NullValueHandling = NullValueHandling.Ignore)]
        public string Label { get; internal set; }

        /// <summary>
        /// Gets the list of rules
        /// </summary>
        [JsonProperty("values", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<string> Values { get; internal set; }

        /// <summary>
        /// Gets whether the user has to fill out this field
        /// </summary>
        [JsonProperty("required", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsRequired { get; internal set; }

        public DiscordGuildMembershipScreeningField(string label, string[] values, bool? required = true)
        {
            this.Label = label;
            this.Values = values;
            this.IsRequired = required;
        }

        internal DiscordGuildMembershipScreeningField() { }
    }
}
