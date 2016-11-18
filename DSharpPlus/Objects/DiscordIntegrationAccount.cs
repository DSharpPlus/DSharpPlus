using Newtonsoft.Json;

namespace DSharpPlus
{
    /// <summary>
    /// 
    /// </summary>
    public class DiscordIntegrationAccount : SnowflakeObject
    {
        /// <summary>
        /// Name of the account
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; internal set; }
    }
}
