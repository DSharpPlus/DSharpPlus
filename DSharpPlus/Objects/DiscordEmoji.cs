using System.Collections.Generic;
using Newtonsoft.Json;

namespace DSharpPlus
{
    /// <summary>
    /// 
    /// </summary>
    public class DiscordEmoji : SnowflakeObject
    {
        /// <summary>
        /// Emoji Name
        /// </summary>
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; internal set; }
        /// <summary>
        /// Roles this emoji is active for
        /// </summary>
        [JsonProperty("roles", NullValueHandling = NullValueHandling.Ignore)]
        public List<ulong> Roles { get; internal set; }
        /// <summary>
        /// Whether this emoji must be wrapped in colons
        /// </summary>
        [JsonProperty("require_colons", NullValueHandling = NullValueHandling.Ignore)]
        public bool RequireColons { get; internal set; }
        /// <summary>
        /// Whether this emoji is managed
        /// </summary>
        [JsonProperty("managed", NullValueHandling = NullValueHandling.Ignore)]
        public bool Managed { get; internal set; }

        /// <summary>
        /// Returns a string representation of this emoji.
        /// </summary>
        /// <returns>String representation of this emoji.</returns>
        public override string ToString()
        {
            if (this.Id != 0)
                return $"<:{this.Name}:{this.Id}>";
            return this.Name;
        }
    }
}
