using Newtonsoft.Json;

namespace DSharpPlus
{
    /// <summary>
    /// 
    /// </summary>
    public class DiscordRole : SnowflakeObject
    {
        /// <summary>
        /// Role name
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; internal set; }
        /// <summary>
        /// Integer representation of a hexadecimal color code
        /// </summary>
        [JsonProperty("color")]
        public int Color { get; internal set; }
        /// <summary>
        /// Whether this role is pinned
        /// </summary>
        [JsonProperty("hoist")]
        public bool Hoist { get; internal set; }
        /// <summary>
        /// Position of this role
        /// </summary>
        [JsonProperty("position")]
        public int Position { get; internal set; }
        /// <summary>
        /// Permission bit set
        /// </summary>
        [JsonProperty("permissions")]
        public int Permissions { get; internal set; }
        /// <summary>
        /// Whether this role is managed by an integration
        /// </summary>
        [JsonProperty("managed")]
        public bool Managed { get; internal set; }
        /// <summary>
        /// Whether this role is mentionable
        /// </summary>
        [JsonProperty("mentionable")]
        public bool Mentionable { get; internal set; }
    }
}
