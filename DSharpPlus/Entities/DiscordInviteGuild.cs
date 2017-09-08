using System.Globalization;
using Newtonsoft.Json;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents a guild to which the user is invited.
    /// </summary>
    public class DiscordInviteGuild : SnowflakeObject
    {
        /// <summary>
        /// Gets the name of the guild.
        /// </summary>
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; internal set; }

        /// <summary>
        /// Gets the hash of guild's invite splash.
        /// </summary>
        [JsonProperty("splash_name", NullValueHandling = NullValueHandling.Ignore)]
        internal string SplashHash { get; set; }

        /// <summary>
        /// Gets the URL of guild's invite splash.
        /// </summary>
        [JsonIgnore]
        public string SplashUrl => !string.IsNullOrWhiteSpace(this.SplashHash) ? $"https://cdn.discordapp.com/splashes/{this.Id.ToString(CultureInfo.InvariantCulture)}/{SplashHash}.jpg" : null;
    }
}
