using System;
using System.Collections.Generic;
using DSharpPlus.Core.Attributes;
using DSharpPlus.Core.RestEntities;
using System.Text.Json.Serialization;

namespace DSharpPlus.Core.GatewayEntities.Payloads
{
    /// <summary>
    /// Sent when a guild member is updated. This will also fire when the user object of a guild member changes.
    /// </summary>
    [DiscordGatewayPayload("GUILD_MEMBER_UPDATE")]
    public sealed record DiscordGuildMemberUpdatePayload
    {
        /// <summary>
        /// The id of the guild.
        /// </summary>
        [JsonPropertyName("guild_id")]
        public DiscordSnowflake GuildId { get; init; } = null!;

        /// <summary>
        /// User's role ids.
        /// </summary>
        [JsonPropertyName("roles")]
        public IReadOnlyList<DiscordSnowflake> Roles { get; init; } = Array.Empty<DiscordSnowflake>();

        /// <summary>
        /// The user.
        /// </summary>
        [JsonPropertyName("user")]
        public DiscordUser User { get; init; } = null!;

        /// <summary>
        /// The nickname of the user in the guild.
        /// </summary>
        [JsonPropertyName("nick")]
        public Optional<string?> Nick { get; init; }

        /// <summary>
        /// The member's guild avatar hash.
        /// </summary>
        [JsonPropertyName("avatar")]
        public string? Avatar { get; init; }

        /// <summary>
        /// When the user joined the guild.
        /// </summary>
        [JsonPropertyName("joined_at")]
        public DateTimeOffset? JoinedAt { get; init; }

        /// <summary>
        /// When the user started boosting the guild.
        /// </summary>
        [JsonPropertyName("premium_since")]
        public Optional<DateTimeOffset?> PremiumSince { get; init; }

        /// <summary>
        /// Whether the user is deafened in voice channels.
        /// </summary>
        [JsonPropertyName("deaf")]
        public Optional<bool> Deaf { get; init; }

        /// <summary>
        /// Whether the user is muted in voice channels.
        /// </summary>
        [JsonPropertyName("mute")]
        public Optional<bool> Mute { get; init; }

        /// <summary>
        /// Whether the user has not yet passed the guild's Membership Screening requirements.
        /// </summary>
        [JsonPropertyName("pending")]
        public Optional<bool> Pending { get; init; }

        /// <summary>
        /// When the user's timeout will expire and the user will be able to communicate in the guild again, null or a time in the past if the user is not timed out.
        /// </summary>
        [JsonPropertyName("communication_disabled_until")]
        public Optional<DateTimeOffset?> CommunicationDisabledUntil { get; init; }
    }
}
