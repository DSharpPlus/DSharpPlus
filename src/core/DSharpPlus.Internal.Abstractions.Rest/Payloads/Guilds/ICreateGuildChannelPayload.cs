// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Entities;
using DSharpPlus.Internal.Abstractions.Models;

namespace DSharpPlus.Internal.Abstractions.Rest.Payloads;

/// <summary>
/// Represents a payload to <c>POST /guilds/:guild-id/channels</c>.
/// </summary>
public interface ICreateGuildChannelPayload
{
    /// <summary>
    /// The name of the channel to be created.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The channel type.
    /// </summary>
    public Optional<DiscordChannelType?> Type { get; }

    /// <summary>
    /// The channel topic.
    /// </summary>
    public Optional<string?> Topic { get; }

    /// <summary>
    /// The voice channel bitrate.
    /// </summary>
    public Optional<int?> Bitrate { get; }

    /// <summary>
    /// The voice channel user limit.
    /// </summary>
    public Optional<int?> UserLimit { get; }

    /// <summary>
    /// The user slowmode in seconds.
    /// </summary>
    public Optional<int?> RateLimitPerUser { get; }

    /// <summary>
    /// The sorting position in the channel list for this channel.
    /// </summary>
    public Optional<int?> Position { get; }

    /// <summary>
    /// The permission overwrites for this channel.
    /// </summary>
    public Optional<IReadOnlyList<IChannelOverwrite>?> PermissionOverwrites { get; }

    /// <summary>
    /// The category channel ID for this channel.
    /// </summary>
    public Optional<Snowflake?> ParentId { get; }

    /// <summary>
    /// Indicates whether this channel is a NSFW channel.
    /// </summary>
    public Optional<bool?> Nsfw { get; }

    /// <summary>
    /// Channel voice region ID for this voice/stage channel.
    /// </summary>
    public Optional<string?> RtcRegion { get; }

    /// <summary>
    /// Indicates the camera video quality mode of this channel.
    /// </summary>
    public Optional<DiscordVideoQualityMode?> VideoQualityMode { get; }

    /// <summary>
    /// The default auto archive duration clients use for newly created threads in this channel.
    /// </summary>
    public Optional<int?> DefaultAutoArchiveDuration { get; }

    /// <summary>
    /// Default reaction for threads in this forum channel.
    /// </summary>
    public Optional<IDefaultReaction?> DefaultReactionEmoji { get; }

    /// <summary>
    /// The set of tags that can be used in this forum channel.
    /// </summary>
    public Optional<IReadOnlyList<IForumTag>?> AvailableTags { get; }

    /// <summary>
    /// The default sort order for this forum channel.
    /// </summary>
    public Optional<DiscordForumSortOrder?> DefaultSortOrder { get; }

    /// <summary>
    /// The default forum layout view used to display posts in this forum channel.
    /// </summary>
    public Optional<DiscordForumLayoutType?> DefaultForumLayout { get; }

    /// <summary>
    /// The initial slowmode to set on newly created threads in this channel. This field is only used on creation
    /// and does not live update.
    /// </summary>
    public Optional<int?> DefaultThreadRateLimitPerUser { get; }
}
