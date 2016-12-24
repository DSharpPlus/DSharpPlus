using System;
using Newtonsoft.Json;

namespace DSharpPlus
{
    /// <summary>
    /// 
    /// </summary>
    public class DiscordInviteMetadata
    {
        /// <summary>
        /// User who created the invite
        /// </summary>
        [JsonProperty("inviter", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordUser Inviter { get; internal set; }
        /// <summary>
        /// Number of times this invite has been used
        [JsonProperty("uses", NullValueHandling = NullValueHandling.Ignore)]
        public int Uses { get; internal set; }
        /// <summary>
        /// Max number of times this invite can be used
        /// </summary>
        [JsonProperty("max_uses", NullValueHandling = NullValueHandling.Ignore)]
        public int MaxUses { get; internal set; }
        /// <summary>
        /// Duration after which the invite expires
        /// </summary>
        [JsonProperty("max_age", NullValueHandling = NullValueHandling.Ignore)]
        public int MaxAge { get; internal set; }
        /// <summary>
        /// Whether this invite only grants temporary membership
        /// </summary>
        [JsonProperty("temporary", NullValueHandling = NullValueHandling.Ignore)]
        public bool Temporary { get; internal set; }
        /// <summary>
        /// When this invite was created
        /// </summary>
        [JsonProperty("created_at", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime CreatedAt { get; internal set; }
        /// <summary>
        /// Whether this invite is revoked
        /// </summary>
        [JsonProperty("revoked", NullValueHandling = NullValueHandling.Ignore)]
        public bool Revoked { get; internal set; }
    }
}
