using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DSharpPlus
{
    /// <summary>
    /// 
    /// </summary>
    public class DiscordMember
    {
        /// <summary>
        /// User object
        /// </summary>
        [JsonProperty("user")]
        public DiscordUser User { get; internal set; }
        /// <summary>
        /// This users guild nickname
        /// </summary>
        [JsonProperty("nickname")]
        public string Nickname { get; internal set; }
        /// <summary>
        /// List of role object id's
        /// </summary>
        [JsonProperty("roles")]
        public List<ulong> Roles { get; internal set; }
        /// <summary>
        /// Date the user joined the guild
        /// </summary>
        [JsonProperty("joined_at")]
        public DateTime JoinedAt { get; internal set; }
        /// <summary>
        /// If the user is deafened
        /// </summary>
        [JsonProperty("is_deafened")]
        public bool IsDeafened { get; internal set; }
        /// <summary>
        /// If the user is muted
        /// </summary>
        [JsonProperty("is_muted")]
        public bool IsMuted { get; internal set; }

        public async Task<DiscordDMChannel> SendDM() => await DiscordClient.InternalCreateDM(User.ID);
    }
}
