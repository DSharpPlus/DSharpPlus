using System;
using System.Collections.Generic;
using DSharpPlus.Core.Enums;
using Newtonsoft.Json;

namespace DSharpPlus.Core.RestEntities
{
    public sealed record DiscordTeamMember
    {
        /// <summary>
        /// The user's <see cref="DiscordMembershipState">membership state</see> on the team.
        /// </summary>
        [JsonProperty("membership_state", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordMembershipState MembershipState { get; init; }

        /// <summary>
        /// Will always be <c>["*"]</c>.
        /// </summary>
        [JsonProperty("permissions", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<string> Permissions { get; init; } = Array.Empty<string>();

        /// <summary>
        /// The id of the parent team of which they are a member.
        /// </summary>
        [JsonProperty("team_id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake TeamId { get; init; } = null!;

        /// <summary>
        /// The avatar, discriminator, id, and username of the user.
        /// </summary>
        [JsonProperty("user", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordUser User { get; init; } = null!;

        public static implicit operator ulong(DiscordTeamMember teamMember) => teamMember.User.Id;
        public static implicit operator DiscordSnowflake(DiscordTeamMember teamMember) => teamMember.User.Id;
    }
}
