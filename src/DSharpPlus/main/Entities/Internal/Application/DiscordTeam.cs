using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DSharpPlus.Core.Entities
{
    public sealed record DiscordTeam
    {
        /// <summary>
        /// A hash of the image of the team's icon.
        /// </summary>
        [JsonPropertyName("icon")]
        public string? Icon { get; init; }

        /// <summary>
        /// The unique id of the team.
        /// </summary>
        [JsonPropertyName("id")]
        public DiscordSnowflake Id { get; init; } = null!;

        /// <summary>
        /// The members of the team.
        /// </summary>
        [JsonPropertyName("members")]
        public IReadOnlyList<DiscordTeamMember> Members { get; init; } = Array.Empty<DiscordTeamMember>();

        /// <summary>
        /// The name of the team.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; init; } = null!;

        /// <summary>
        /// The user id of the current team owner.
        /// </summary>
        [JsonPropertyName("owner_user_id")]
        public DiscordSnowflake OwnerUserId { get; init; } = null!;

        public static implicit operator ulong(DiscordTeam team) => team.Id;
        public static implicit operator DiscordSnowflake(DiscordTeam team) => team.Id;
    }
}
