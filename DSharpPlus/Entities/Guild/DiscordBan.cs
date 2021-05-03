using DSharpPlus.Net.Abstractions;
using Newtonsoft.Json;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents a Discord ban
    /// </summary>
    public class DiscordBan
    {
        /// <summary>
        /// Gets the reason for the ban
        /// </summary>
        [JsonProperty("reason", NullValueHandling = NullValueHandling.Ignore)]
        public string Reason { get; internal set; }

        /// <summary>
        /// Gets the banned user
        /// </summary>
        [JsonIgnore]
        public DiscordUser User { get; internal set; }
        [JsonProperty("user", NullValueHandling = NullValueHandling.Ignore)]
        internal TransportUser RawUser { get; set; }

        internal DiscordBan() { }
    }
}
