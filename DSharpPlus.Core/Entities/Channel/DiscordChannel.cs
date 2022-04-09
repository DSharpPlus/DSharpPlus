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
using System.Text.Json.Serialization;
using DSharpPlus.Core.Enums;

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
        [JsonPropertyName("id")]
        public DiscordSnowflake Id { get; init; } = null!;

        /// <summary>
        /// The <see cref="DiscordChannelType">type of channel</see>.
        /// </summary>
        [JsonPropertyName("type")]
        public DiscordChannelType Type { get; internal set; }

        /// <summary>
        /// The id of the guild (may be missing for some channel objects received over gateway guild dispatches).
        /// </summary>
        [JsonPropertyName("guild_id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Optional<DiscordSnowflake> GuildId { get; init; }

        /// <summary>
        /// Sorting position of the channel.
        /// </summary>
        [JsonPropertyName("position")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Optional<int> Position { get; internal set; }

        /// <summary>
        /// Explicit permission overwrites for members and roles.
        /// </summary>
        [JsonPropertyName("permission_overwrites")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Optional<DiscordChannelOverwrite[]> PermissionOverwrites { get; internal set; }

        /// <summary>
        /// The name of the channel (1-100 characters).
        /// </summary>
        [JsonPropertyName("name")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Optional<string> Name { get; internal set; }

        /// <summary>
        /// The channel topic (0-1024 characters).
        /// </summary>
        [JsonPropertyName("topic")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Optional<string?> Topic { get; internal set; }

        /// <summary>
        /// Whether the channel is nsfw.
        /// </summary>
        [JsonPropertyName("nsfw")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Optional<bool> NSFW { get; internal set; }

        /// <summary>
        /// The id of the last message sent in this channel (may not point to an existing or valid message).
        /// </summary>
        [JsonPropertyName("last_message_id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Optional<DiscordSnowflake?> LastMessageId { get; internal set; }

        /// <summary>
        /// The bitrate (in bits) of the voice channel.
        /// </summary>
        [JsonPropertyName("bitrate")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Optional<int> Bitrate { get; internal set; }

        /// <summary>
        /// The user limit of the voice channel.
        /// </summary>
        [JsonPropertyName("user_limit")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Optional<int> UserLimit { get; internal set; }

        /// <summary>
        /// Amount of seconds a user has to wait before sending another message (0-21600); bots, as well as users with the permission <see cref="DiscordPermissions.ManageMessages"/> or <see cref="DiscordPermissions.ManageChannels"/>, are unaffected
        /// </summary>
        [JsonPropertyName("rate_limit_per_user")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Optional<int> RateLimitPerUser { get; internal set; }

        /// <summary>
        /// The recipients of the DM.
        /// </summary>
        [JsonPropertyName("recipients")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Optional<DiscordUser[]> Recipients { get; internal set; }

        /// <summary>
        /// Icon hash of the group DM.
        /// </summary>
        [JsonPropertyName("icon")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Optional<string?> Icon { get; internal set; }

        /// <summary>
        /// Id of the creator of the group DM or thread.
        /// </summary>
        [JsonPropertyName("owner_id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Optional<DiscordSnowflake> OwnerId { get; internal set; }

        /// <summary>
        /// Application id of the group DM creator if it is bot-created.
        /// </summary>
        [JsonPropertyName("application_id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Optional<DiscordSnowflake> ApplicationId { get; internal set; }

        /// <summary>
        /// For guild channels: id of the parent category for a channel (each parent category can contain up to 50 channels), for threads: id of the text channel this thread was created.
        /// </summary>
        [JsonPropertyName("parent_id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Optional<DiscordSnowflake?> ParentId { get; internal set; }

        /// <summary>
        /// When the last pinned message was pinned. This may be null in events such as GUILD_CREATE when a message is not pinned.
        /// </summary>
        [JsonPropertyName("last_pin_timestamp")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Optional<DateTimeOffset> LastPinTimestamp { get; internal set; }

        /// <summary>
        /// Voice region id for the voice channel, automatic when set to null.
        /// </summary>
        [JsonPropertyName("rtc_region")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Optional<string?> RtcRegion { get; internal set; }

        /// <summary>
        /// The camera video quality mode of the voice channel, <see cref="DiscordChannelVideoQualityMode.Auto"/> when not present.
        /// </summary>
        [JsonPropertyName("video_quality_mode")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Optional<DiscordChannelVideoQualityMode> VideoQualityMode { get; internal set; }

        /// <summary>
        /// An approximate count of messages in a thread, stops counting at 50.
        /// </summary>
        [JsonPropertyName("message_count")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Optional<int> MessageCount { get; internal set; }

        /// <summary>
        /// An approximate count of users in a thread, stops counting at 50.
        /// </summary>
        [JsonPropertyName("member_count")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Optional<int> MemberCount { get; internal set; }

        /// <summary>
        /// Thread-specific fields not needed by other channels.
        /// </summary>
        [JsonPropertyName("thread_metadata")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Optional<DiscordChannelThreadMetadata> ThreadMetadata { get; internal set; }

        /// <summary>
        /// Thread member object for the current user, if they have joined the thread, only included on certain API endpoints.
        /// </summary>
        [JsonPropertyName("thread_member")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Optional<DiscordThreadMember> ThreadMember { get; internal set; }

        /// <summary>
        /// Default duration that the clients (not the API) will use for newly created threads, in minutes, to automatically archive the thread after recent activity, can be set to: 60, 1440, 4320, 10080.
        /// </summary>
        [JsonPropertyName("default_auto_archive_duration")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Optional<DiscordThreadAutoArchiveDuration> DefaultAutoArchiveDuration { get; internal set; }

        /// <summary>
        /// Computed permissions for the invoking user in the channel, including overwrites, only included when part of the <c>resolved</c> data received on a slash command interaction
        /// </summary>
        [JsonPropertyName("permissions")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Optional<DiscordPermissions> Permissions { get; internal set; }

        /// <summary>
        /// When the thread is newly created. Only sent in gateway payloads.
        /// </summary>
        [JsonPropertyName("newly_created")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Optional<bool> NewlyCreated { get; init; }

        public static implicit operator ulong(DiscordChannel channel) => channel.Id;
        public static implicit operator DiscordSnowflake(DiscordChannel channel) => channel.Id;
    }
}
