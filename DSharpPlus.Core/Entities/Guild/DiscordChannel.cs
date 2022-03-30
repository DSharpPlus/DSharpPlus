// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2022 DSharpPlus Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using DSharpPlus.Core.Enums;
using Newtonsoft.Json;

namespace DSharpPlus.Core.Entities
{
    /// <summary>
    /// Represents a guild or DM channel within Discord.
    /// </summary>
    public sealed record DiscordChannel
    {
        /// <summary>
        /// The id of this channel.
        /// </summary>
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake Id { get; init; } = null!;

        /// <summary>
        /// The <see cref="DiscordChannelType">type of channel</see>.
        /// </summary>
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordChannelType Type { get; internal set; }

        /// <summary>
        /// The id of the guild (may be missing for some channel objects received over gateway guild dispatches).
        /// </summary>
        [JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordSnowflake> GuildId { get; init; }

        /// <summary>
        /// Sorting position of the channel.
        /// </summary>
        [JsonProperty("position", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<int> Position { get; internal set; }

        /// <summary>
        /// Explicit permission overwrites for members and roles.
        /// </summary>
        [JsonProperty("permission_overwrites", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordChannelOverwrite[]> PermissionOverwrites { get; internal set; }

        /// <summary>
        /// The name of the channel (1-100 characters).
        /// </summary>
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<string> Name { get; internal set; }

        /// <summary>
        /// The channel topic (0-1024 characters).
        /// </summary>
        [JsonProperty("topic", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<string?> Topic { get; internal set; }

        /// <summary>
        /// Whether the channel is nsfw.
        /// </summary>
        [JsonProperty("nsfw", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<bool> NSFW { get; internal set; }

        /// <summary>
        /// The id of the last message sent in this channel (may not point to an existing or valid message).
        /// </summary>
        [JsonProperty("last_message_id", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordSnowflake?> LastMessageId { get; internal set; }

        /// <summary>
        /// The bitrate (in bits) of the voice channel.
        /// </summary>
        [JsonProperty("bitrate", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<int> Bitrate { get; internal set; }

        /// <summary>
        /// The user limit of the voice channel.
        /// </summary>
        [JsonProperty("user_limit", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<int> UserLimit { get; internal set; }

        /// <summary>
        /// Amount of seconds a user has to wait before sending another message (0-21600); bots, as well as users with the permission <see cref="DiscordPermissions.ManageMessages"/> or <see cref="DiscordPermissions.ManageChannels"/>, are unaffected
        /// </summary>
        [JsonProperty("rate_limit_per_user", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<int> RateLimitPerUser { get; internal set; }

        /// <summary>
        /// The recipients of the DM.
        /// </summary>
        [JsonProperty("recipients", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordUser[]> Recipients { get; internal set; }

        /// <summary>
        /// Icon hash of the group DM.
        /// </summary>
        [JsonProperty("icon", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<string?> IconHash { get; internal set; }

        /// <summary>
        /// Id of the creator of the group DM or thread.
        /// </summary>
        [JsonProperty("owner_id", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordSnowflake> OwnerId { get; internal set; }

        /// <summary>
        /// Application id of the group DM creator if it is bot-created.
        /// </summary>
        [JsonProperty("application_id", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordSnowflake> ApplicationId { get; internal set; }

        /// <summary>
        /// For guild channels: id of the parent category for a channel (each parent category can contain up to 50 channels), for threads: id of the text channel this thread was created.
        /// </summary>
        [JsonProperty("parent_id", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordSnowflake?> ParentId { get; internal set; }

        /// <summary>
        /// When the last pinned message was pinned. This may be null in events such as GUILD_CREATE when a message is not pinned.
        /// </summary>
        [JsonProperty("last_pin_timestamp", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DateTimeOffset> LastPinTimestamp { get; internal set; }

        /// <summary>
        /// Voice region id for the voice channel, automatic when set to null.
        /// </summary>
        [JsonProperty("rtc_region", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<string?> RtcRegion { get; internal set; }

        /// <summary>
        /// The camera video quality mode of the voice channel, <see cref="DiscordChannelVideoQualityMode.Auto"/> when not present.
        /// </summary>
        [JsonProperty("video_quality_mode", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordChannelVideoQualityMode> VideoQualityMode { get; internal set; }

        /// <summary>
        /// An approximate count of messages in a thread, stops counting at 50.
        /// </summary>
        [JsonProperty("message_count", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<int> MessageCount { get; internal set; }

        /// <summary>
        /// An approximate count of users in a thread, stops counting at 50.
        /// </summary>
        [JsonProperty("member_count", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<int> MemberCount { get; internal set; }

        /// <summary>
        /// Thread-specific fields not needed by other channels.
        /// </summary>
        [JsonProperty("thread_metadata", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordThreadMetadata> ThreadMetadata { get; internal set; }

        /// <summary>
        /// Thread member object for the current user, if they have joined the thread, only included on certain API endpoints.
        /// </summary>
        [JsonProperty("thread_member", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordThreadMember> ThreadMember { get; internal set; }

        /// <summary>
        /// Default duration that the clients (not the API) will use for newly created threads, in minutes, to automatically archive the thread after recent activity, can be set to: 60, 1440, 4320, 10080.
        /// </summary>
        [JsonProperty("default_auto_archive_duration", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordThreadAutoArchiveDuration> DefaultAutoArchiveDuration { get; internal set; }

        /// <summary>
        /// Computed permissions for the invoking user in the channel, including overwrites, only included when part of the <c>resolved</c> data received on a slash command interaction
        /// </summary>
        [JsonProperty("permissions", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordPermissions> Permissions { get; internal set; }
    }
}
