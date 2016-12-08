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
<<<<<<< HEAD
        [JsonProperty("username", NullValueHandling = NullValueHandling.Ignore)]
=======
        [JsonProperty("username")]
>>>>>>> master
        public string Username { get; internal set; }
        /// <summary>
        /// The user's 4-digit tag
        /// </summary>
<<<<<<< HEAD
        [JsonProperty("discriminator", NullValueHandling = NullValueHandling.Ignore)]
=======
        [JsonProperty("discriminator")]
>>>>>>> master
        public int Discriminator { get; internal set; }
        /// <summary>
        /// The user's avatar hash
        /// </summary>
<<<<<<< HEAD
        [JsonProperty("avatar", NullValueHandling = NullValueHandling.Ignore)]
=======
        [JsonProperty("avatar")]
>>>>>>> master
        public string AvatarHash { get; internal set; }
        /// <summary>
        /// The user's avatar url
        /// </summary>
        [JsonIgnore]
        public string AvatarUrl => $"https://cdn.discordapp.com/avatars/{ID}/{AvatarHash}.jpg";
        /// <summary>
        /// Whether the user belongs to an oauth2 application
        /// </summary>
<<<<<<< HEAD
        [JsonProperty("is_bot", NullValueHandling = NullValueHandling.Ignore)]
=======
        [JsonProperty("is_bot")]
>>>>>>> master
        public bool IsBot { get; internal set; }
        /// <summary>
        /// Whether the user has tho factor enabled
        /// </summary>
<<<<<<< HEAD
        [JsonProperty("mfa_enabled", NullValueHandling = NullValueHandling.Ignore)]
=======
        [JsonProperty("mfa_enabled")]
>>>>>>> master
        public bool? MFAEnabled { get; internal set; }
        /// <summary>
        /// Whether the email on this account has been verified
        /// </summary>
<<<<<<< HEAD
        [JsonProperty("verified", NullValueHandling = NullValueHandling.Ignore)]
=======
        [JsonProperty("verified")]
>>>>>>> master
        public bool? Verified { get; internal set; }
        /// <summary>
        /// The user's email
        /// </summary>
<<<<<<< HEAD
        [JsonProperty("email", NullValueHandling = NullValueHandling.Ignore)]
=======
        [JsonProperty("email")]
>>>>>>> master
        public string Email { get; internal set; }
    }
}
