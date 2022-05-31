using System;
using System.Collections.Generic;
using DSharpPlus.Core.Attributes;
using DSharpPlus.Core.RestEntities;
using Newtonsoft.Json;

namespace DSharpPlus.Core.GatewayEntities.Payloads
{
    /// <summary>
    /// Sent in response to Guild Request Members. You can use the chunk_index and chunk_count to calculate how many chunks are left for your request.
    /// </summary>
    [DiscordGatewayPayload("GUILD_MEMBERS_CHUNK")]
    public sealed record DiscordGuildMembersChunkPayload
    {
        /// <summary>
        /// The id of the guild.
        /// </summary>
        [JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake GuildId { get; init; } = null!;

        /// <summary>
        /// A set of guild members.
        /// </summary>
        [JsonProperty("members", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<DiscordGuildMember> Members { get; init; } = Array.Empty<DiscordGuildMember>();

        /// <summary>
        /// The chunk index in the expected chunks for this response (0 <= chunk_index < chunk_count).
        /// </summary>
        [JsonProperty("chunk_index", NullValueHandling = NullValueHandling.Ignore)]
        public int ChunkIndex { get; init; }

        /// <summary>
        /// The total number of expected chunks for this response.
        /// </summary>
        [JsonProperty("chunk_count", NullValueHandling = NullValueHandling.Ignore)]
        public int ChunkCount { get; init; }

        /// <summary>
        /// If passing an invalid id to REQUEST_GUILD_MEMBERS, it will be returned here.
        /// </summary>
        [JsonProperty("not_found", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<IReadOnlyList<DiscordSnowflake>> NotFound { get; init; }

        /// <summary>
        /// If passing true to REQUEST_GUILD_MEMBERS, presences of the returned members will be here.
        /// </summary>
        [JsonProperty("presences", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordUpdatePresencePayload> Presences { get; init; }

        /// <summary>
        /// The nonce used in the Guild Members Request.
        /// </summary>
        [JsonProperty("nonce", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<string> Nonce { get; init; }
    }
}
