using System.Collections.Generic;
using DSharpPlus.Core.Attributes;
using DSharpPlus.Core.RestEntities;
using System.Text.Json.Serialization;

namespace DSharpPlus.Core.GatewayEntities.Payloads
{
    /// <summary>
    /// Sent when anyone is added to or removed from a thread. If the current user does not have the <see cref="Enums.DiscordGatewayIntents.GuildMembers"/>, then this event will only be sent if the current user was added to or removed from the thread.
    /// </summary>
    /// <remarks>
    /// In this gateway event, the thread member objects will also include the <see cref="DiscordGuildMember"/> and nullable <see cref="DiscordUpdatePresencePayload"/> for each added thread member.
    /// </remarks>
    [DiscordGatewayPayload("THREAD_MEMBERS_UPDATE")]
    public sealed record DiscordThreadMembersUpdatePayload
    {
        /// <summary>
        /// The id of the thread.
        /// </summary>
        [JsonPropertyName("id")]
        public DiscordSnowflake Id { get; init; } = null!;

        /// <summary>
        /// The id of the guild.
        /// </summary>
        [JsonPropertyName("guild_id")]
        public DiscordSnowflake GuildId { get; init; } = null!;

        /// <summary>
        /// The approximate number of members in the thread, capped at 50.
        /// </summary>
        [JsonPropertyName("member_count")]
        public int MemberCount { get; init; }

        /// <summary>
        /// The users who were added to the thread.
        /// </summary>
        [JsonPropertyName("added_members")]
        public Optional<IReadOnlyList<DiscordThreadMember>> AddedMembers { get; init; }

        /// <summary>
        /// The id of the users who were removed from the thread.
        /// </summary>
        [JsonPropertyName("removed_member_ids")]
        public Optional<IReadOnlyList<DiscordSnowflake>> RemovedMemberIds { get; init; }
    }
}
