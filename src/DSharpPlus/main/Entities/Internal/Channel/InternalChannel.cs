using System;
using System.Collections.Generic;
using DSharpPlus.Core.Attributes;
using DSharpPlus.Core.Enums;
using System.Text.Json.Serialization;

namespace DSharpPlus.Core.Entities
{
    /// <summary>
    /// Represents a guild or DM channel within Internal.
    /// </summary>
    [InternalGatewayPayload("CHANNEL_CREATE", "CHANNEL_UPDATE", "CHANNEL_DELETE", "THREAD_CREATE", "THREAD_UPDATE", "THREAD_DELETE")]
    public sealed record InternalChannel
    {
        /// <summary>
        /// The id of this channel.
        /// </summary>
        [JsonPropertyName("id")]
        public InternalSnowflake Id { get; init; } = null!;

        /// <summary>
        /// The <see cref="InternalChannelType">type of channel</see>.
        /// </summary>
        [JsonPropertyName("type")]
        public InternalChannelType Type { get; init; }

        /// <summary>
        /// The id of the guild (may be missing for some channel objects received over gateway guild dispatches).
        /// </summary>
        [JsonPropertyName("guild_id")]
        public Optional<InternalSnowflake> GuildId { get; init; }

        /// <summary>
        /// The sorting position of the channel.
        /// </summary>
        [JsonPropertyName("position")]
        public Optional<int> Position { get; init; }

        /// <summary>
        /// Explicit permission overwrites for members and roles.
        /// </summary>
        [JsonPropertyName("permission_overwrites")]
        public Optional<IReadOnlyList<InternalChannelOverwrite>> PermissionOverwrites { get; init; }

        /// <summary>
        /// The name of the channel (1-100 characters).
        /// </summary>
        [JsonPropertyName("name")]
        public Optional<string?> Name { get; init; }

        /// <summary>
        /// The channel topic (0-1024 characters).
        /// </summary>
        [JsonPropertyName("topic")]
        public Optional<string?> Topic { get; init; }

        /// <summary>
        /// Whether the channel is nsfw.
        /// </summary>
        [JsonPropertyName("nsfw")]
        public Optional<bool> NSFW { get; init; }

        /// <summary>
        /// The id of the last message sent in this channel (or thread for <see cref="InternalChannelType.GuildForum"/> channels) (may not point to an existing or valid message or thread)
        /// </summary>
        [JsonPropertyName("last_message_id")]
        public Optional<InternalSnowflake?> LastMessageId { get; init; }

        /// <summary>
        /// The bitrate (in bits) of the voice channel.
        /// </summary>
        [JsonPropertyName("bitrate")]
        public Optional<int> Bitrate { get; init; }

        /// <summary>
        /// The user limit of the voice channel.
        /// </summary>
        [JsonPropertyName("user_limit")]
        public Optional<int> UserLimit { get; init; }

        /// <summary>
        /// The amount of seconds a user has to wait before sending another message (0-21600); bots, as well as users with the permission <see cref="InternalPermissions.ManageMessages"/> or <see cref="InternalPermissions.ManageChannels"/>, are unaffected
        /// </summary>
        [JsonPropertyName("rate_limit_per_user")]
        public Optional<int> RateLimitPerUser { get; init; }

        /// <summary>
        /// The recipients of the DM.
        /// </summary>
        [JsonPropertyName("recipients")]
        public Optional<IReadOnlyList<InternalUser>> Recipients { get; init; }

        /// <summary>
        /// Icon hash of the group DM.
        /// </summary>
        [JsonPropertyName("icon")]
        public Optional<string?> Icon { get; init; }

        /// <summary>
        /// Id of the creator of the group DM or thread.
        /// </summary>
        [JsonPropertyName("owner_id")]
        public Optional<InternalSnowflake> OwnerId { get; init; }

        /// <summary>
        /// Application id of the group DM creator if it is bot-created.
        /// </summary>
        [JsonPropertyName("application_id")]
        public Optional<InternalSnowflake> ApplicationId { get; init; }

        /// <summary>
        /// For guild channels: id of the parent category for a channel (each parent category can contain up to 50 channels), for threads: id of the text channel this thread was created.
        /// </summary>
        [JsonPropertyName("parent_id")]
        public Optional<InternalSnowflake?> ParentId { get; init; }

        /// <summary>
        /// When the last pinned message was pinned. This may be null in events such as GUILD_CREATE when a message is not pinned.
        /// </summary>
        [JsonPropertyName("last_pin_timestamp")]
        public Optional<DateTimeOffset?> LastPinTimestamp { get; init; }

        /// <summary>
        /// Voice region id for the voice channel, automatic when set to null.
        /// </summary>
        [JsonPropertyName("rtc_region")]
        public Optional<string?> RtcRegion { get; init; }

        /// <summary>
        /// The camera video quality mode of the voice channel, <see cref="InternalChannelVideoQualityMode.Auto"/> when not present.
        /// </summary>
        [JsonPropertyName("video_quality_mode")]
        public Optional<InternalChannelVideoQualityMode> VideoQualityMode { get; init; } = InternalChannelVideoQualityMode.Auto;

        /// <summary>
        /// An approximate count of messages in a thread, stops counting at 50.
        /// </summary>
        [JsonPropertyName("message_count")]
        public Optional<int> MessageCount { get; init; }

        /// <summary>
        /// An approximate count of users in a thread, stops counting at 50.
        /// </summary>
        [JsonPropertyName("member_count")]
        public Optional<int> MemberCount { get; init; }

        /// <summary>
        /// Thread-specific fields not needed by other channels.
        /// </summary>
        [JsonPropertyName("thread_metadata")]
        public Optional<InternalChannelThreadMetadata> ThreadMetadata { get; init; }

        /// <summary>
        /// Thread member object for the current user, if they have joined the thread, only included on certain API endpoints.
        /// </summary>
        [JsonPropertyName("member")]
        public Optional<InternalThreadMember> Member { get; init; }

        /// <summary>
        /// Default duration that the clients (not the API) will use for newly created threads, in minutes, to automatically archive the thread after recent activity, can be set to: 60, 1440, 4320, 10080.
        /// </summary>
        [JsonPropertyName("default_auto_archive_duration")]
        public Optional<InternalThreadAutoArchiveDuration> DefaultAutoArchiveDuration { get; init; }

        /// <summary>
        /// Computed permissions for the invoking user in the channel, including overwrites, only included when part of the <c>resolved</c> data received on a slash command interaction
        /// </summary>
        [JsonPropertyName("permissions")]
        public Optional<InternalPermissions> Permissions { get; init; }

        /// <summary>
        /// When the thread is newly created. Only sent in gateway payloads.
        /// </summary>
        [JsonPropertyName("newly_created")]
        public Optional<bool> NewlyCreated { get; init; }

        /// <summary>
        /// Channel flags combined as a bitfield.
        /// </summary>
        [JsonPropertyName("flags")]
        public Optional<InternalChannelFlags> Flags { get; init; }

        public static implicit operator ulong(InternalChannel channel) => channel.Id;
        public static implicit operator InternalSnowflake(InternalChannel channel) => channel.Id;
    }
}
