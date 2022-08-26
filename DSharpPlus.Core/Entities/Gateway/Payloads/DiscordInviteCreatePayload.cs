using System;
using DSharpPlus.Core.Attributes;
using DSharpPlus.Core.Enums;
using System.Text.Json.Serialization;

namespace DSharpPlus.Core.Entities.Gateway.Payloads
{
    /// <summary>
    /// Sent when a new invite to a channel is created.
    /// </summary>
    [DiscordGatewayPayload("INVITE_CREATE")]
    public sealed record DiscordInviteCreatePayload
    {
        /// <summary>
        /// The channel the invite is for.
        /// </summary>
        [JsonPropertyName("channel_id")]
        public DiscordSnowflake ChannelId { get; init; } = null!;

        /// <summary>
        /// The unique invite code.
        /// </summary>
        [JsonPropertyName("code")]
        public string Code { get; init; } = null!;

        /// <summary>
        /// The time at which the invite was created.
        /// </summary>
        [JsonPropertyName("created_at")]
        public DateTimeOffset CreatedAt { get; init; }

        /// <summary>
        /// The guild of the invite.
        /// </summary>
        [JsonPropertyName("guild_id")]
        public Optional<DiscordSnowflake> GuildId { get; init; }

        /// <summary>
        /// The user that created the invite.
        /// </summary>
        [JsonPropertyName("inviter")]
        public Optional<DiscordUser> Inviter { get; init; }

        /// <summary>
        /// How long the invite is valid for (in seconds).
        /// </summary>
        [JsonPropertyName("max_age")]
        public int MaxAge { get; init; }

        /// <summary>
        /// The maximum number of times the invite can be used.
        /// </summary>
        [JsonPropertyName("max_uses")]
        public int MaxUses { get; init; }

        /// <summary>
        /// The type of target for this voice channel invite.
        /// </summary>
        [JsonPropertyName("target_type")]
        public Optional<DiscordGuildInviteTargetType> TargetType { get; init; }

        /// <summary>
        /// The user whose stream to display for this voice channel stream invite.
        /// </summary>
        [JsonPropertyName("target_user")]
        public Optional<DiscordUser> TargetUser { get; init; }

        /// <summary>
        /// The embedded application to open for this voice channel embedded application invite.
        /// </summary>
        [JsonPropertyName("target_application")]
        public Optional<DiscordApplication> TargetApplication { get; init; }

        /// <summary>
        /// Whether or not the invite is temporary (invited users will be kicked on disconnect unless they're assigned a role).
        /// </summary>
        [JsonPropertyName("temporary")]
        public bool Temporary { get; init; }

        /// <summary>
        /// How many times the invite has been used (always will be 0).
        /// </summary>
        [JsonPropertyName("uses")]
        public int Uses { get; init; }
    }
}
