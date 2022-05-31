using System;
using DSharpPlus.Core.Attributes;
using Newtonsoft.Json;

namespace DSharpPlus.Core.RestEntities
{
    /// <summary>
    /// A thread member is used to indicate whether a user has joined a thread or not.
    /// </summary>
    /// <remarks>
    /// The <see cref="Id"/> and <see cref="UserId"/> fields are omitted on the member sent within each thread in the <c>GUILD_CREATE</c> event
    /// </remarks>
    [DiscordGatewayPayload("THREAD_MEMBER_UPDATE")]
    public sealed record DiscordThreadMember
    {
        /// <summary>
        /// The id of the thread.
        /// </summary>
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordSnowflake> Id { get; init; }

        /// <summary>
        /// The id of the user.
        /// </summary>
        [JsonProperty("user_id", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordSnowflake> UserId { get; init; }

        /// <summary>
        /// The time the current user last joined the thread.
        /// </summary>
        [JsonProperty("join_timestamp", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset JoinTimestamp { get; init; }

        /// <summary>
        /// Any user-thread settings, currently only used for notifications.
        /// </summary>
        [JsonProperty("flags", NullValueHandling = NullValueHandling.Ignore)]
        public int Flags { get; init; }

        /// <summary>
        /// The id of the guild.
        /// </summary>
        /// <remarks>
        /// Only sent on the ThreadMemberUpdate gateway payload.
        /// </remarks>
        [JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordSnowflake> GuildId { get; init; }
    }
}
