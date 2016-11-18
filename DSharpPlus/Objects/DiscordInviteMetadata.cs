using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        [JsonProperty("inviter")]
        public DiscordUser Inviter { get; internal set; }
        /// <summary>
        /// Number of times this invite has been used
        /// </summary>
        [JsonProperty("uses")]
        public int Uses { get; internal set; }
        /// <summary>
        /// Max number of times this invite can be used
        /// </summary>
        [JsonProperty("max_uses")]
        public int MaxUses { get; internal set; }
        /// <summary>
        /// Duration after which the invite expires
        /// </summary>
        [JsonProperty("max_age")]
        public int MaxAge { get; internal set; }
        /// <summary>
        /// Whether this invite only grants temporary membership
        /// </summary>
        [JsonProperty("temporary")]
        public bool Temporary { get; internal set; }
        /// <summary>
        /// When this invite was created
        /// </summary>
        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; internal set; }
        /// <summary>
        /// Whether this invite is revoked
        /// </summary>
        [JsonProperty("revoked")]
        public bool Revoked { get; internal set; }
    }
}
