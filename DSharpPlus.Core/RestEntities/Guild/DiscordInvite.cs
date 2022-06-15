using System;
using DSharpPlus.Core.Enums;
using Newtonsoft.Json;

namespace DSharpPlus.Core.RestEntities
{
    /// <summary>
    /// Represents a code that when used, adds a user to a guild or group DM channel.
    /// </summary>
    public sealed record DiscordInvite
    {
        /// <summary>
        /// The invite code (unique ID).
        /// </summary>
        [JsonProperty("code", NullValueHandling = NullValueHandling.Ignore)]
        public string Code { get; init; } = null!;

        /// <summary>
        /// The guild this invite is for.
        /// </summary>
        [JsonProperty("guild", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordGuild> Guild { get; init; }

        /// <summary>
        /// The channel this invite is for.
        /// </summary>
        [JsonProperty("channel", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordChannel? Channel { get; init; }

        /// <summary>
        /// The user who created the invite.
        /// </summary>
        [JsonProperty("inviter", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordUser> Inviter { get; init; }

        /// <summary>
        /// The type of target for this voice channel invite.
        /// </summary>
        [JsonProperty("target_type", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordInviteTargetType> TargetType { get; init; }

        /// <summary>
        /// The user whose stream to display for this voice channel stream invite.
        /// </summary>
        [JsonProperty("target_user", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordUser> TargetUser { get; init; }

        /// <summary>
        /// The embedded application to open for this voice channel embedded application invite.
        /// </summary>
        [JsonProperty("target_application", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordApplication> TargetApplication { get; init; }

        /// <summary>
        /// The approximate count of online members, returned from the GET /invites/:code endpoint when with_counts is true.
        /// </summary>
        [JsonProperty("approximate_presence_count", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<int> ApproximatePresenceCount { get; init; }

        /// <summary>
        /// The approximate count of total members, returned from the GET /invites/:code endpoint when with_counts is true.
        /// </summary>
        [JsonProperty("approximate_member_count", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<int> ApproximateMemberCount { get; init; }

        /// <summary>
        /// The expiration date of this invite, returned from the GET /invites/:code endpoint when with_expiration is true.
        /// </summary>
        [JsonProperty("expires_at", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DateTimeOffset?> ExpiresAt { get; init; }

        /// <summary>
        /// The stage instance data if there is a public stage instance in the stage channel this invite is for (deprecated).
        /// </summary>
        [Obsolete("The stage instance data if there is a public stage instance in the stage channel this invite is for (deprecated).", false)]
        [JsonProperty("stage_instance", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordStageInstance> StageInstance { get; init; }

        /// <summary>
        /// The guild scheduled event data, only included if <see cref="DiscordGuildScheduledEventUser.GuildScheduledEventId"/> contains a valid guild scheduled event id.
        /// </summary>
        [JsonProperty("guild_scheduled_event", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordGuildScheduledEvent> GuildScheduledEvent { get; init; }

        #region Invite Metadata Object
        // Why does this "extend" the object instead of just modifying the object directly?

        /// <summary>
        /// The number of times this invite has been used.
        /// </summary>
        [JsonProperty("uses", NullValueHandling = NullValueHandling.Ignore)]
        public int Uses { get; init; }

        /// <summary>
        /// The max number of times this invite can be used.
        /// </summary>
        [JsonProperty("max_uses", NullValueHandling = NullValueHandling.Ignore)]
        public int MaxUses { get; init; }

        /// <summary>
        /// The duration (in seconds) after which the invite expires.
        /// </summary>
        [JsonProperty("max_age", NullValueHandling = NullValueHandling.Ignore)]
        public int MaxAge { get; init; }

        /// <summary>
        /// Whether this invite only grants temporary membership.
        /// </summary>
        [JsonProperty("temporary", NullValueHandling = NullValueHandling.Ignore)]
        public bool Temporary { get; init; }

        /// <summary>
        /// When this invite was created.
        /// </summary>
        [JsonProperty("created_at", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset CreatedAt { get; init; }
        #endregion
    }
}
