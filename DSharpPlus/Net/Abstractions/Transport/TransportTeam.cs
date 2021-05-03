using System.Collections.Generic;
using Newtonsoft.Json;

namespace DSharpPlus.Net.Abstractions
{
    internal sealed class TransportTeam
    {
        [JsonProperty("id")]
        public ulong Id { get; set; }

        [JsonProperty("name", NullValueHandling = NullValueHandling.Include)]
        public string Name { get; set; }

        [JsonProperty("icon", NullValueHandling = NullValueHandling.Include)]
        public string IconHash { get; set; }

        [JsonProperty("owner_user_id")]
        public ulong OwnerId { get; set; }

        [JsonProperty("members", NullValueHandling = NullValueHandling.Include)]
        public IEnumerable<TransportTeamMember> Members { get; set; }

        internal TransportTeam() { }
    }

    internal sealed class TransportTeamMember
    {
        [JsonProperty("membership_state")]
        public int MembershipState { get; set; }

        [JsonProperty("permissions", NullValueHandling = NullValueHandling.Include)]
        public IEnumerable<string> Permissions { get; set; }

        [JsonProperty("team_id")]
        public ulong TeamId { get; set; }

        [JsonProperty("user", NullValueHandling = NullValueHandling.Include)]
        public TransportUser User { get; set; }

        internal TransportTeamMember() { }
    }
}
