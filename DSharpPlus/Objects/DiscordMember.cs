using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace DSharpPlus
{
    public class DiscordMember
    {
        [JsonProperty("user")]
        public DiscordUser User { get; internal set; }
        [JsonProperty("nickname")]
        public string Nickname { get; internal set; }
        [JsonProperty("roles")]
        public List<ulong> Roles { get; internal set; }
        [JsonProperty("joined_at")]
        public DateTime JoinedAt { get; internal set; }
        [JsonProperty("is_deafened")]
        public bool IsDeafened { get; internal set; }
        [JsonProperty("is_muted")]
        public bool IsMuted { get; internal set; }
    }
}
