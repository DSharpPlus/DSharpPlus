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
<<<<<<< HEAD
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
=======
        [JsonProperty("name")]
>>>>>>> master
        public string Name { get; internal set; }
        /// <summary>
        /// Hash of the guild splash
        /// </summary>
<<<<<<< HEAD
        [JsonProperty("splash_name", NullValueHandling = NullValueHandling.Ignore)]
=======
        [JsonProperty("splash_name")]
>>>>>>> master
        public string SplashName { get; internal set; }
    }
}
