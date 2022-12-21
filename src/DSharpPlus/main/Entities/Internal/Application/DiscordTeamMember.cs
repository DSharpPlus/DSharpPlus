using System;
using System.Collections.Generic;
using DSharpPlus.Core.Enums;
using System.Text.Json.Serialization;

namespace DSharpPlus.Core.Entities
{
    public sealed record DiscordTeamMember
    {
        /// <summary>
        /// The user's <see cref="DiscordMembershipState">membership state</see> on the team.
        /// </summary>
        [JsonPropertyName("membership_state")]
        public DiscordMembershipState MembershipState { get; init; }

        /// <summary>
        /// Will always be <c>["*"]</c>.
        /// </summary>
        [JsonPropertyName("permissions")]
        public IReadOnlyList<string> Permissions { get; init; } = Array.Empty<string>();

        /// <summary>
        /// The id of the parent team of which they are a member.
        /// </summary>
        [JsonPropertyName("team_id")]
        public DiscordSnowflake TeamId { get; init; } = null!;

        /// <summary>
        /// The avatar, discriminator, id, and username of the user.
        /// </summary>
        [JsonPropertyName("user")]
        public DiscordUser User { get; init; } = null!;

        public static implicit operator ulong(DiscordTeamMember teamMember) => teamMember.User.Id;
        public static implicit operator DiscordSnowflake(DiscordTeamMember teamMember) => teamMember.User.Id;
    }
}
