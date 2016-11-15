using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSharpPlus
{
    public class DiscordInviteMetadata
    {
        [JsonProperty("inviter")]
        public DiscordUser Inviter { get; internal set; }
        [JsonProperty("uses")]
        public int Uses { get; internal set; }
        [JsonProperty("max_uses")]
        public int MaxUses { get; internal set; }
        [JsonProperty("max_age")]
        public int MaxAge { get; internal set; }
        [JsonProperty("temporary")]
        public bool Temporary { get; internal set; }
        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; internal set; }
        [JsonProperty("revoked")]
        public bool Revoked { get; internal set; }
    }
}
