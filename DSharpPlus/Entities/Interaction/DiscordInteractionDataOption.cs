using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents parameters for interaction commands. 
    /// </summary>
    public sealed class DiscordInteractionDataOption
    {
        /// <summary>
        /// Gets the name of this interaction parameter.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; internal set; }

        /// <summary>
        /// Gets the type of this interaction parameter.
        /// </summary>
        [JsonProperty("type")]
        public ApplicationCommandOptionType Type { get; internal set; }

        [JsonProperty("value")]
        internal string _value { get; set; }

        /// <summary>
        /// Gets the value of this interaction parameter. 
        /// <para>This can be cast to a <see langword="long"/>, <see langword="bool"></see>, <see langword="string"></see> or <see langword="ulong"/> depending on the <see cref="Type"/></para>
        /// </summary>
        [JsonIgnore]
        public object Value
        {
            get
            {
                return this.Type switch
                {
                    ApplicationCommandOptionType.Boolean => bool.Parse(this._value),
                    ApplicationCommandOptionType.Integer => long.Parse(this._value),
                    ApplicationCommandOptionType.String => this._value,
                    ApplicationCommandOptionType.Channel => ulong.Parse(this._value),
                    ApplicationCommandOptionType.User => ulong.Parse(this._value),
                    ApplicationCommandOptionType.Role => ulong.Parse(this._value),
                    _ => this._value,
                };
            }
        }

        /// <summary>
        /// Gets the additional parameters if this parameter is a subcommand.
        /// </summary>
        [JsonProperty("options", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<DiscordInteractionDataOption> Options { get; internal set; }
    }
}
