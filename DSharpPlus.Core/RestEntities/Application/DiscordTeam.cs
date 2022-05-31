using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DSharpPlus.Core.RestEntities
{
    public sealed record DiscordTeam
    {
        /// <summary>
        /// A hash of the image of the team's icon.
        /// </summary>
        [JsonProperty("icon", NullValueHandling = NullValueHandling.Ignore)]
        public string? Icon { get; init; }

        /// <summary>
        /// The unique id of the team.
        /// </summary>
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake Id { get; init; } = null!;

        /// <summary>
        /// The members of the team.
        /// </summary>
        [JsonProperty("members", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<DiscordTeamMember> Members { get; init; } = Array.Empty<DiscordTeamMember>();

        /// <summary>
        /// The name of the team.
        /// </summary>
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; init; } = null!;

        /// <summary>
        /// The user id of the current team owner.
        /// </summary>
        [JsonProperty("owner_user_id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake OwnerUserId { get; init; } = null!;

        public static implicit operator ulong(DiscordTeam team) => team.Id;
        public static implicit operator DiscordSnowflake(DiscordTeam team) => team.Id;
    }
}
