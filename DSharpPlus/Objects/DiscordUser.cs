using DSharpPlus.Objects.Transport;
using Newtonsoft.Json;

namespace DSharpPlus
{
    /// <summary>
    /// 
    /// </summary>
    public class DiscordUser : SnowflakeObject
    {
        internal DiscordUser() { }
        internal DiscordUser(TransportUser transport)
        {
            this.Id = transport.Id;
            this.Username = transport.Username;
            this.DiscriminatorInt = transport.DiscriminatorInt;
            this.AvatarHash = transport.AvatarHash;
            this.IsBot = transport.IsBot;
            this.MFAEnabled = transport.MFAEnabled;
            this.Verified = transport.Verified;
            this.Email = transport.Email;
        }

        /// <summary>
        /// The user's username
        /// </summary>
        [JsonProperty("username", NullValueHandling = NullValueHandling.Ignore)]
        public string Username { get; internal set; }
        /// <summary>
        /// The user's 4-digit tag
        /// </summary>
        [JsonProperty("discriminator", NullValueHandling = NullValueHandling.Ignore)]
        internal int DiscriminatorInt { get; set; }
        /// <summary>
        /// Gets the user's 4-digit tag.
        /// </summary>
        [JsonIgnore]
        public string Discriminator => this.DiscriminatorInt.ToString("0000");
        /// <summary>
        /// The user's avatar hash
        /// </summary>
        [JsonProperty("avatar", NullValueHandling = NullValueHandling.Ignore)]
        public string AvatarHash { get; internal set; }
        /// <summary>
        /// The user's avatar url
        /// </summary>
        [JsonIgnore]
        public string AvatarUrl => AvatarHash.StartsWith("a_")? $"https://cdn.discordapp.com/avatars/{Id}/{AvatarHash}.gif" : $"https://cdn.discordapp.com/avatars/{Id}/{AvatarHash}.jpg";
        /// <summary>
        /// Whether the user belongs to an oauth2 application
        /// </summary>
        [JsonProperty("bot", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsBot { get; internal set; }
        /// <summary>
        /// Whether the user has tho factor enabled
        /// </summary>
        [JsonProperty("mfa_enabled", NullValueHandling = NullValueHandling.Ignore)]
        public bool? MFAEnabled { get; internal set; }
        /// <summary>
        /// Whether the email on this account has been verified
        /// </summary>
        [JsonProperty("verified", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Verified { get; internal set; }
        /// <summary>
        /// The user's email
        /// </summary>
        [JsonProperty("email", NullValueHandling = NullValueHandling.Ignore)]
        public string Email { get; internal set; }
        /// <summary>
        /// Mentions the user similar to how a client would
        /// </summary>
        public string Mention => Formatter.Mention(this);
        /// <summary>
        /// This user's presence.
        /// </summary>
        public DiscordPresence Presence => this.Discord.InternalGetUserPresence(Id);
    }
}
