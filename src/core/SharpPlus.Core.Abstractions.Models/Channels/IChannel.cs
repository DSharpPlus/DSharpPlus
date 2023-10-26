// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;

using DSharpPlus.Core.Abstractions.Models;
using DSharpPlus.Entities;

namespace DSharpPlus.Core.Models;

/// <inheritdoc cref="IChannel" />
public sealed record Channel : IChannel
{
    /// <inheritdoc/>
    public required Snowflake Id { get; init; }

    /// <inheritdoc/>
    public required DiscordChannelType Type { get; init; }

    /// <inheritdoc/>
    public Optional<Snowflake> GuildId { get; init; }

    /// <inheritdoc/>
    public Optional<int> Position { get; init; }

    /// <inheritdoc/>
    public Optional<IReadOnlyList<IChannelOverwrite>> PermissionOverwrites { get; init; }

    /// <inheritdoc/>
    public Optional<string?> Name { get; init; }

    /// <inheritdoc/>
    public Optional<string?> Topic { get; init; }

    /// <inheritdoc/>
    public Optional<bool> Nsfw { get; init; }

    /// <inheritdoc/>
    public Optional<Snowflake?> LastMessageId { get; init; }

    /// <inheritdoc/>
    public Optional<int> Bitrate { get; init; }

    /// <inheritdoc/>
    public Optional<int> UserLimit { get; init; }

    /// <inheritdoc/>
    public Optional<int> RateLimitPerUser { get; init; }

    /// <inheritdoc/>
    public Optional<IReadOnlyList<IUser>> Recipients { get; init; }

    /// <inheritdoc/>
    public Optional<string?> Icon { get; init; }

    /// <inheritdoc/>
    public Optional<Snowflake> OwnerId { get; init; }

    /// <inheritdoc/>
    public Optional<Snowflake> ApplicationId { get; init; }

    /// <inheritdoc/>
    public Optional<bool> Managed { get; init; }

    /// <inheritdoc/>
    public Optional<Snowflake?> ParentId { get; init; }

    /// <inheritdoc/>
    public Optional<DateTimeOffset?> LastPinTimestamp { get; init; }

    /// <inheritdoc/>
    public Optional<string?> RtcRegion { get; init; }

    /// <inheritdoc/>
    public Optional<DiscordVideoQualityMode> VideoQualityMode { get; init; }

    /// <inheritdoc/>
    public Optional<int> MessageCount { get; init; }

    /// <inheritdoc/>
    public Optional<int> MemberCount { get; init; }

    /// <inheritdoc/>
    public Optional<IThreadMetadata> ThreadMetadata { get; init; }

    /// <inheritdoc/>
    public Optional<IThreadMember> Member { get; init; }

    /// <inheritdoc/>
    public Optional<int> DefaultAutoArchiveDuration { get; init; }

    /// <inheritdoc/>
    public Optional<DiscordPermissions> Permissions { get; init; }

    /// <inheritdoc/>
    public Optional<DiscordChannelFlags> Flags { get; init; }

    /// <inheritdoc/>
    public Optional<int> TotalMessageSent { get; init; }

    /// <inheritdoc/>
    public Optional<IReadOnlyList<IForumTag>> AvailableTags { get; init; }

    /// <inheritdoc/>
    public Optional<IReadOnlyList<Snowflake>> AppliedTags { get; init; }

    /// <inheritdoc/>
    public Optional<IDefaultReaction?> DefaultReactionEmoji { get; init; }

    /// <inheritdoc/>
    public Optional<int> DefaultThreadRateLimitPerUser { get; init; }

    /// <inheritdoc/>
    public Optional<DiscordForumSortOrder?> DefaultSortOrder { get; init; }

    /// <inheritdoc/>
    public Optional<DiscordForumLayoutType> DefaultForumLayout { get; init; }
}