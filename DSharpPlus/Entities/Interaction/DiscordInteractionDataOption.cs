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
                switch (this.Type)
                {
                    case ApplicationCommandOptionType.Boolean:
                        return bool.Parse(this._value);
                    case ApplicationCommandOptionType.Integer:
                        return long.Parse(this._value);
                    case ApplicationCommandOptionType.String:
                        return this._value;
                    case ApplicationCommandOptionType.Channel:
                        return ulong.Parse(this._value);
                    case ApplicationCommandOptionType.User:
                        return ulong.Parse(this._value);
                    case ApplicationCommandOptionType.Role:
                        return ulong.Parse(this._value);
                    default:
                        return this._value;
                }
            }
        }

        /// <summary>
        /// Gets the additional parameters if this parameter is a subcommand.
        /// </summary>
        [JsonProperty("options", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<DiscordInteractionDataOption> Options { get; internal set; }
    }
}
