using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DSharpPlus
{
    /// <summary>
    /// 
    /// </summary>
    public class DiscordUser : SnowflakeObject
    {
        /// <summary>
        /// The user's username
        /// </summary>
        [JsonProperty("username")]
        public string Username { get; internal set; }
        /// <summary>
        /// The user's 4-digit tag
        /// </summary>
        [JsonProperty("discriminator")]
        public int Discriminator { get; internal set; }
        /// <summary>
        /// The user's avatar hash
        /// </summary>
        [JsonProperty("avatar")]
        public string AvatarHash { get; internal set; }
        /// <summary>
        /// The user's avatar url
        /// </summary>
        [JsonIgnore]
        public string AvatarUrl => $"https://cdn.discordapp.com/avatars/{ID}/{AvatarHash}.jpg";
        /// <summary>
        /// Whether the user belongs to an oauth2 application
        /// </summary>
        [JsonProperty("is_bot")]
        public bool IsBot { get; internal set; }
        /// <summary>
        /// Whether the user has tho factor enabled
        /// </summary>
        [JsonProperty("mfa_enabled")]
        public bool? MFAEnabled { get; internal set; }
        /// <summary>
        /// Whether the email on this account has been verified
        /// </summary>
        [JsonProperty("verified")]
        public bool? Verified { get; internal set; }
        /// <summary>
        /// The user's email
        /// </summary>
        [JsonProperty("email")]
        public string Email { get; internal set; }
    }
}
