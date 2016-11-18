using Newtonsoft.Json;
using System.Collections.Generic;

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
        [JsonProperty("name")]
        public string Name { get; internal set; }
        /// <summary>
        /// Roles this emoji is active for
        /// </summary>
        [JsonProperty("roles")]
        public List<DiscordRole> Roles { get; internal set; }
        /// <summary>
        /// Whether this emoji must be wrapped in colons
        /// </summary>
        [JsonProperty("require_colons")]
        public bool RequireColons { get; internal set; }
        /// <summary>
        /// Whether this emoji is managed
        /// </summary>
        [JsonProperty("managed")]
        public bool Managed { get; internal set; }
    }
}
