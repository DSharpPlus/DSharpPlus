using System;
using DSharpPlus.Core.Enums;
using System.Text.Json.Serialization;

namespace DSharpPlus.Core.Entities
{
    /// <summary>
    /// Represents a code that when used, adds a user to a guild or group DM channel.
    /// </summary>
    public sealed record DiscordInvite
    {
        /// <summary>
        /// The invite code (unique ID).
        /// </summary>
        [JsonPropertyName("code")]
        public string Code { get; init; } = null!;

        /// <summary>
        /// The guild this invite is for.
        /// </summary>
        [JsonPropertyName("guild")]
        public Optional<DiscordGuild> Guild { get; init; }

        /// <summary>
        /// The channel this invite is for.
        /// </summary>
        [JsonPropertyName("channel")]
        public DiscordChannel? Channel { get; init; }

        /// <summary>
        /// The user who created the invite.
        /// </summary>
        [JsonPropertyName("inviter")]
        public Optional<DiscordUser> Inviter { get; init; }

        /// <summary>
        /// The type of target for this voice channel invite.
        /// </summary>
        [JsonPropertyName("target_type")]
        public Optional<DiscordInviteTargetType> TargetType { get; init; }

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
        /// The approximate count of online members, returned from the GET /invites/:code endpoint when with_counts is true.
        /// </summary>
        [JsonPropertyName("approximate_presence_count")]
        public Optional<int> ApproximatePresenceCount { get; init; }

        /// <summary>
        /// The approximate count of total members, returned from the GET /invites/:code endpoint when with_counts is true.
        /// </summary>
        [JsonPropertyName("approximate_member_count")]
        public Optional<int> ApproximateMemberCount { get; init; }

        /// <summary>
        /// The expiration date of this invite, returned from the GET /invites/:code endpoint when with_expiration is true.
        /// </summary>
        [JsonPropertyName("expires_at")]
        public Optional<DateTimeOffset?> ExpiresAt { get; init; }

        /// <summary>
        /// The stage instance data if there is a public stage instance in the stage channel this invite is for (deprecated).
        /// </summary>
        [Obsolete("The stage instance data if there is a public stage instance in the stage channel this invite is for (deprecated).", false)]
        [JsonPropertyName("stage_instance")]
        public Optional<DiscordStageInstance> StageInstance { get; init; }

        /// <summary>
        /// The guild scheduled event data, only included if <see cref="DiscordGuildScheduledEventUser.GuildScheduledEventId"/> contains a valid guild scheduled event id.
        /// </summary>
        [JsonPropertyName("guild_scheduled_event")]
        public Optional<DiscordGuildScheduledEvent> GuildScheduledEvent { get; init; }

        #region Invite Metadata Object
        // Why does this "extend" the object instead of just modifying the object directly?

        /// <summary>
        /// The number of times this invite has been used.
        /// </summary>
        [JsonPropertyName("uses")]
        public int Uses { get; init; }

        /// <summary>
        /// The max number of times this invite can be used.
        /// </summary>
        [JsonPropertyName("max_uses")]
        public int MaxUses { get; init; }

        /// <summary>
        /// The duration (in seconds) after which the invite expires.
        /// </summary>
        [JsonPropertyName("max_age")]
        public int MaxAge { get; init; }

        /// <summary>
        /// Whether this invite only grants temporary membership.
        /// </summary>
        [JsonPropertyName("temporary")]
        public bool Temporary { get; init; }

        /// <summary>
        /// When this invite was created.
        /// </summary>
        [JsonPropertyName("created_at")]
        public DateTimeOffset CreatedAt { get; init; }
        #endregion
    }
}
