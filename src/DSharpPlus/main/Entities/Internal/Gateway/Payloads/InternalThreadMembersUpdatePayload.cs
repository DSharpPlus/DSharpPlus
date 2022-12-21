using System.Collections.Generic;
using DSharpPlus.Core.Attributes;
using DSharpPlus.Core.Entities;
using System.Text.Json.Serialization;

namespace DSharpPlus.Core.Entities.Gateway.Payloads
{
    /// <summary>
    /// Sent when anyone is added to or removed from a thread. If the current user does not have the <see cref="Enums.InternalGatewayIntents.GuildMembers"/>, then this event will only be sent if the current user was added to or removed from the thread.
    /// </summary>
    /// <remarks>
    /// In this gateway event, the thread member objects will also include the <see cref="InternalGuildMember"/> and nullable <see cref="InternalUpdatePresencePayload"/> for each added thread member.
    /// </remarks>
    [InternalGatewayPayload("THREAD_MEMBERS_UPDATE")]
    public sealed record InternalThreadMembersUpdatePayload
    {
        /// <summary>
        /// The id of the thread.
        /// </summary>
        [JsonPropertyName("id")]
        public InternalSnowflake Id { get; init; } = null!;

        /// <summary>
        /// The id of the guild.
        /// </summary>
        [JsonPropertyName("guild_id")]
        public InternalSnowflake GuildId { get; init; } = null!;

        /// <summary>
        /// The approximate number of members in the thread, capped at 50.
        /// </summary>
        [JsonPropertyName("member_count")]
        public int MemberCount { get; init; }

        /// <summary>
        /// The users who were added to the thread.
        /// </summary>
        [JsonPropertyName("added_members")]
        public Optional<IReadOnlyList<InternalThreadMember>> AddedMembers { get; init; }

        /// <summary>
        /// The id of the users who were removed from the thread.
        /// </summary>
        [JsonPropertyName("removed_member_ids")]
        public Optional<IReadOnlyList<InternalSnowflake>> RemovedMemberIds { get; init; }
    }
}
