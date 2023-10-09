// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;

using DSharpPlus.Entities;

namespace DSharpPlus.Core.Abstractions.Models;

/// <summary>
/// Represents a partial channel of any given type.
/// </summary>
public interface IPartialChannel
{
    /// <summary>
    /// The snowflake identifier of this channel.
    /// </summary>
    public Optional<Snowflake> Id { get; }

    /// <summary>
    /// The type of this channel.
    /// </summary>
    public Optional<DiscordChannelType> Type { get; }

    /// <summary>
    /// The snowflake identifier of the guild this channel belongs to, if it is a guild channel.
    /// </summary>
    public Optional<Snowflake> GuildId { get; }

    /// <summary>
    /// The sorting position in the guild channel list of this channel, if it is a guild channel.
    /// </summary>
    public Optional<int> Position { get; }

    /// <summary>
    /// A collection of explicit permission overwrites for members and roles.
    /// </summary>
    public Optional<IReadOnlyList<IChannelOverwrite>> PermissionOverwrites { get; }

    /// <summary>
    /// The name of this channel, 1 to 100 characters.
    /// </summary>
    public Optional<string?> Name { get; }

    /// <summary>
    /// The channel topic/description. For <seealso cref="DiscordChannelType.GuildForum"/> and
    /// <seealso cref="DiscordChannelType.GuildMedia"/>, up to 4096 characters are allowed, for all other 
    /// types up to 1024 characters.
    /// </summary>
    public Optional<string?> Topic { get; }

    /// <summary>
    /// Indicates whether the channel is considered a NSFW channel.
    /// </summary>
    public Optional<bool> Nsfw { get; }

    /// <summary>
    /// The snowflake identifier of the last message sent in this channel. This is a thread for forum and media
    /// channels, and may not point to an existing or valid message or thread.
    /// </summary>
    public Optional<Snowflake?> LastMessageId { get; }

    /// <summary>
    /// The bitrate of this channel, if it is a voice channel.
    /// </summary>
    public Optional<int> Bitrate { get; }

    /// <summary>
    /// The user limit of this channel, if it is a voice channel.
    /// </summary>
    public Optional<int> UserLimit { get; }

    /// <summary>
    /// The slowmode of the current channel in seconds, between 0 and 21600. Bots, as well as users
    /// with manage messages or manage channel permissions, are unaffected.
    /// </summary>
    public Optional<int> RateLimitPerUser { get; }

    /// <summary>
    /// The recipients of this DM channel.
    /// </summary>
    public Optional<IReadOnlyList<IUser>> Recipients { get; }

    /// <summary>
    /// The icon hash of this group DM channel.
    /// </summary>
    public Optional<string?> Icon { get; }

    /// <summary>
    /// The snowflake identifier of the creator of this group DM or thread channel.
    /// </summary>
    public Optional<Snowflake> OwnerId { get; }

    /// <summary>
    /// The snowflake identifier of the application that created this group DM, if it was created by a bot.
    /// </summary>
    public Optional<Snowflake> ApplicationId { get; }

    /// <summary>
    /// Indicates whether this channel is managed by an application via OAuth2.
    /// </summary>
    public Optional<bool> Managed { get; }

    /// <summary>
    /// The parent channel of the current channel; either the containing category if this is a standalone
    /// guild channel, or the containing text channel if this is a thread channel.
    /// </summary>
    public Optional<Snowflake?> ParentId { get; }

    /// <summary>
    /// The timestamp at which the last message was pinned.
    /// </summary>
    public Optional<DateTimeOffset?> LastPinTimestamp { get; }

    /// <summary>
    /// The voice region ID of this voice channel, automatic when set to null.
    /// </summary>
    public Optional<string?> RtcRegion { get; }

    /// <summary>
    /// The camera video quality mode of this voice channel, automatic when not present.
    /// </summary>
    public Optional<DiscordVideoQualityMode> VideoQualityMode { get; }

    /// <summary>
    /// The number of messages, excluding the original message and deleted messages, in a thread.
    /// </summary>
    public Optional<int> MessageCount { get; }

    /// <summary>
    /// The approximate amount of users in this thread, stops counting at 50.
    /// </summary>
    public Optional<int> MemberCount { get; }

    /// <summary>
    /// A thread-specific metadata object containing data not needed by other channel types.
    /// </summary>
    public Optional<IThreadMetadata> ThreadMetadata { get; }

    /// <summary>
    /// A thread member object for the current user, if they have joined this thread.
    /// </summary>
    public Optional<IThreadMember> Member { get; }

    /// <summary>
    /// The default thread archive duration in minutes, applied to all threads created within this channel
    /// where the default was not overridden on creation.
    /// </summary>
    public Optional<int> DefaultAutoArchiveDuration { get; }

    /// <summary>
    /// Computed permissions for the invoking user in this channel, including permission overwrites. This
    /// is only sent as part of application command resolved data.
    /// </summary>
    public Optional<DiscordPermissions> Permissions { get; }

    /// <summary>
    /// Additional flags for this channel.
    /// </summary>
    public Optional<DiscordChannelFlags> Flags { get; }

    /// <summary>
    /// The total number of messages sent in this thread, including deleted messages.
    /// </summary>
    public Optional<int> TotalMessageSent { get; }

    /// <summary>
    /// The set of tags that can be used in this forum or media channel.
    /// </summary>
    public Optional<IReadOnlyList<IForumTag>> AvailableTags { get; }

    /// <summary>
    /// The snowflake identifiers of the set of tags that have been applied to this forum or media thread channel.
    /// </summary>
    public Optional<IReadOnlyList<Snowflake>> AppliedTags { get; }

    /// <summary>
    /// The default emoji to show in the add reaction button on a thread in this forum or media channel.
    /// </summary>
    public Optional<IDefaultReaction?> DefaultReactionEmoji { get; }

    /// <summary>
    /// The initial slowmode to set on newly created threads in this channel. This is populated at creation
    /// time and does not sync.
    /// </summary>
    public Optional<int> DefaultThreadRateLimitPerUser { get; }

    /// <summary>
    /// The default sort order for posts in this forum or media channel.
    /// </summary>
    public Optional<DiscordForumSortOrder?> DefaultSortOrder { get; }

    /// <summary>
    /// The default layout view used to display posts in this forum channel.
    /// </summary>
    public Optional<DiscordForumLayoutType> DefaultForumLayout { get; }
}
