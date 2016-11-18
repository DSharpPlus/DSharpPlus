using Newtonsoft.Json;

namespace DSharpPlus
{
    /// <summary>
    /// 
    /// </summary>
    public class DiscordInviteGuild : SnowflakeObject
    {
        /// <summary>
        /// Name of the guild
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; internal set; }
        /// <summary>
        /// Hash of the guild splash
        /// </summary>
        [JsonProperty("splash_name")]
        public string SplashName { get; internal set; }
    }
}
