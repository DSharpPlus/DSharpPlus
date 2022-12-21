using System;
using DSharpPlus.Core.Attributes;
using System.Text.Json.Serialization;

namespace DSharpPlus.Core.Entities
{
    /// <summary>
    /// A thread member is used to indicate whether a user has joined a thread or not.
    /// </summary>
    /// <remarks>
    /// The <see cref="Id"/> and <see cref="UserId"/> fields are omitted on the member sent within each thread in the <c>GUILD_CREATE</c> event
    /// </remarks>
    [InternalGatewayPayload("THREAD_MEMBER_UPDATE")]
    public sealed record InternalThreadMember
    {
        /// <summary>
        /// The id of the thread.
        /// </summary>
        [JsonPropertyName("id")]
        public Optional<InternalSnowflake> Id { get; init; }

        /// <summary>
        /// The id of the user.
        /// </summary>
        [JsonPropertyName("user_id")]
        public Optional<InternalSnowflake> UserId { get; init; }

        /// <summary>
        /// The time the current user last joined the thread.
        /// </summary>
        [JsonPropertyName("join_timestamp")]
        public DateTimeOffset JoinTimestamp { get; init; }

        /// <summary>
        /// Any user-thread settings, currently only used for notifications.
        /// </summary>
        [JsonPropertyName("flags")]
        public int Flags { get; init; }

        /// <summary>
        /// The id of the guild.
        /// </summary>
        /// <remarks>
        /// Only sent on the ThreadMemberUpdate gateway payload.
        /// </remarks>
        [JsonPropertyName("guild_id")]
        public Optional<InternalSnowflake> GuildId { get; init; }
    }
}
