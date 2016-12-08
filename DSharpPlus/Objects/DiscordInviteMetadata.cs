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
<<<<<<< HEAD
        [JsonProperty("inviter", NullValueHandling = NullValueHandling.Ignore)]
=======
        [JsonProperty("inviter")]
>>>>>>> master
        public DiscordUser Inviter { get; internal set; }
        /// <summary>
        /// Number of times this invite has been used
        /// </summary>
<<<<<<< HEAD
        [JsonProperty("uses", NullValueHandling = NullValueHandling.Ignore)]
=======
        [JsonProperty("uses")]
>>>>>>> master
        public int Uses { get; internal set; }
        /// <summary>
        /// Max number of times this invite can be used
        /// </summary>
<<<<<<< HEAD
        [JsonProperty("max_uses", NullValueHandling = NullValueHandling.Ignore)]
=======
        [JsonProperty("max_uses")]
>>>>>>> master
        public int MaxUses { get; internal set; }
        /// <summary>
        /// Duration after which the invite expires
        /// </summary>
<<<<<<< HEAD
        [JsonProperty("max_age", NullValueHandling = NullValueHandling.Ignore)]
=======
        [JsonProperty("max_age")]
>>>>>>> master
        public int MaxAge { get; internal set; }
        /// <summary>
        /// Whether this invite only grants temporary membership
        /// </summary>
<<<<<<< HEAD
        [JsonProperty("temporary", NullValueHandling = NullValueHandling.Ignore)]
=======
        [JsonProperty("temporary")]
>>>>>>> master
        public bool Temporary { get; internal set; }
        /// <summary>
        /// When this invite was created
        /// </summary>
<<<<<<< HEAD
        [JsonProperty("created_at", NullValueHandling = NullValueHandling.Ignore)]
=======
        [JsonProperty("created_at")]
>>>>>>> master
        public DateTime CreatedAt { get; internal set; }
        /// <summary>
        /// Whether this invite is revoked
        /// </summary>
<<<<<<< HEAD
        [JsonProperty("revoked", NullValueHandling = NullValueHandling.Ignore)]
=======
        [JsonProperty("revoked")]
>>>>>>> master
        public bool Revoked { get; internal set; }
    }
}
