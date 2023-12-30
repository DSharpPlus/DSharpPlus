// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Internal.Abstractions.Models;
using DSharpPlus.Entities;

namespace DSharpPlus.Internal.Abstractions.Rest.Payloads;

/// <summary>
/// Represents a payload to <c>PATCH /channels/:channel-id</c>.
/// </summary>
public interface IModifyGuildChannelPayload
{
    /// <summary>
    /// The new channel name.
    /// </summary>
    public Optional<string> Name { get; }

    /// <summary>
    /// The new channel type for this channel. Only converting between <seealso cref="DiscordChannelType.GuildText"/> and
    /// <seealso cref="DiscordChannelType.GuildAnnouncement"/> is supported.
    /// </summary>
    public Optional<DiscordChannelType> Type { get; }

    /// <summary>
    /// The new position for this channel in the channel list.
    /// </summary>
    public Optional<int?> Position { get; }

    /// <summary>
    /// The new channel topic.
    /// </summary>
    public Optional<string?> Topic { get; }

    /// <summary>
    /// Indicates whether this channel permits NSFW content.
    /// </summary>
    public Optional<bool?> Nsfw { get; }

    /// <summary>
    /// The new slowmode for this channel in seconds.
    /// </summary>
    public Optional<int?> RateLimitPerUser { get; }

    /// <summary>
    /// The new bitrate for this voice channel.
    /// </summary>
    public Optional<int?> Bitrate { get; }

    /// <summary>
    /// The new user limit for this voice channel. 0 represents no limit, 1 - 99 represents a limit.
    /// </summary>
    public Optional<int?> UserLimit { get; }

    /// <summary>
    /// New permission overwrites for this channel or category.
    /// </summary>
    public Optional<IReadOnlyList<IChannelOverwrite>?> PermissionOverwrites { get; }

    /// <summary>
    /// Snowflake identifier of the new parent category channel.
    /// </summary>
    public Optional<Snowflake?> ParentId { get; }

    /// <summary>
    /// Channel voice region ID, automatic when set to <c>null</c>.
    /// </summary>
    public Optional<string?> RtcRegion { get; }

    /// <summary>
    /// The new camera video quality mode for this channel.
    /// </summary>
    public Optional<DiscordVideoQualityMode?> VideoQualityMode { get; }

    /// <summary>
    /// The new default auto archive duration for threads as used by the discord client.
    /// </summary>
    public Optional<int?> DefaultAutoArchiveDuration { get; }

    /// <summary>
    /// The new channel flags. Currently only <seealso cref="DiscordChannelFlags.RequireTag"/> and
    /// <seealso cref="DiscordChannelFlags.HideMediaDownloadOptions"/> are supported.
    /// </summary>
    public Optional<DiscordChannelFlags> Flags { get; }

    /// <summary>
    /// The set of tags that can be used in this channel.
    /// </summary>
    public Optional<IReadOnlyList<IForumTag>> AvailableTags { get; }

    /// <summary>
    /// The default emoji to react with.
    /// </summary>
    public Optional<IDefaultReaction?> DefaultReactionEmoji { get; }

    /// <summary>
    /// The default slowmode in threads created from this channel.
    /// </summary>
    public Optional<int> DefaultThreadRateLimitPerUser { get; }

    /// <summary>
    /// The default sort order used to order posts in this channel.
    /// </summary>
    public Optional<DiscordForumSortOrder?> DefaultSortOrder { get; }

    /// <summary>
    /// The default layout type used to display posts in this forum.
    /// </summary>
    public Optional<DiscordForumLayoutType> DefaultForumLayout { get; }
}
