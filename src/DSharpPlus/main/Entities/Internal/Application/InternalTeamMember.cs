using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DSharpPlus.Entities.Internal;

public sealed record InternalTeamMember
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
    public required IReadOnlyList<string> Permissions { get; init; } 

    /// <summary>
    /// The id of the parent team of which they are a member.
    /// </summary>
    [JsonPropertyName("team_id")]
    public required Snowflake TeamId { get; init; } 

    /// <summary>
    /// The avatar, discriminator, id, and username of the user.
    /// </summary>
    [JsonPropertyName("user")]
    public required InternalUser User { get; init; }

    public static implicit operator ulong(InternalTeamMember teamMember) => teamMember.User.Id;
    public static implicit operator Snowflake(InternalTeamMember teamMember) => teamMember.User.Id;
}
