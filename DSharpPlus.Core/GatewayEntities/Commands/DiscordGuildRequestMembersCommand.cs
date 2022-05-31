using System.Collections.Generic;
using DSharpPlus.Core.RestEntities;
using Newtonsoft.Json;

namespace DSharpPlus.Core.GatewayEntities.Commands
{
    /// <summary>
    /// Used to request all members for a guild or a list of guilds.
    /// </summary>
    public sealed record DiscordGuildRequestMembersCommand
    {
        /// <summary>
        /// The id of the guild to get members for.
        /// </summary>
        /// <remarks>
        /// Always required.
        /// </remarks>
        [JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake GuildId { get; init; } = null!;

        /// <summary>
        /// The string that username starts with, or an empty string to return all members.
        /// </summary>
        /// <remarks>
        /// Required if <see cref="UserIds"/> is not set.
        /// </remarks>
        [JsonProperty("query", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<string> Query { get; init; }

        /// <summary>
        /// The maximum number of members to send matching the query; a limit of 0 can be used with an empty string query to return all members.
        /// </summary>
        /// <remarks>
        /// Required when <see cref="Query"/> is set.
        /// </remarks>
        [JsonProperty("limit", NullValueHandling = NullValueHandling.Ignore)]
        public int Limit { get; init; }

        /// <summary>
        /// Used to specify if we want the presences of the matched members.
        /// </summary>
        [JsonProperty("presences", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<bool> Presences { get; init; }

        /// <summary>
        /// Used to specify which users you wish to fetch.
        /// </summary>
        /// <remarks>
        /// Required if <see cref="Query"/> is not set.
        /// </remarks>
        [JsonProperty("user_ids", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<IReadOnlyList<DiscordSnowflake>> UserIds { get; init; }

        /// <summary>
        /// Nonce to identify the Guild Members Chunk response.
        /// </summary>
        /// <remarks>
        /// Nonce can only be up to 32 bytes. If you send an invalid nonce it will be ignored and the reply member_chunk(s) will not have a nonce set.
        /// </remarks>
        [JsonProperty("nonce", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<string> Nonce { get; init; }
    }
}
