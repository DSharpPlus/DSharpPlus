// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Entities;
using DSharpPlus.Internal.Abstractions.Models;
using DSharpPlus.Internal.Abstractions.Rest.Payloads;

namespace DSharpPlus.Internal.Rest.Payloads;

/// <inheritdoc cref="ICreateGuildChannelPayload" />
public sealed record CreateGuildChannelPayload : ICreateGuildChannelPayload
{
    /// <inheritdoc/>
    public required string Name { get; init; }

    /// <inheritdoc/>
    public Optional<DiscordChannelType?> Type { get; init; }

    /// <inheritdoc/>
    public Optional<string?> Topic { get; init; }

    /// <inheritdoc/>
    public Optional<int?> Bitrate { get; init; }

    /// <inheritdoc/>
    public Optional<int?> UserLimit { get; init; }

    /// <inheritdoc/>
    public Optional<int?> RateLimitPerUser { get; init; }

    /// <inheritdoc/>
    public Optional<int?> Position { get; init; }

    /// <inheritdoc/>
    public Optional<IReadOnlyList<IChannelOverwrite>?> PermissionOverwrites { get; init; }

    /// <inheritdoc/>
    public Optional<Snowflake?> ParentId { get; init; }

    /// <inheritdoc/>
    public Optional<bool?> Nsfw { get; init; }

    /// <inheritdoc/>
    public Optional<string?> RtcRegion { get; init; }

    /// <inheritdoc/>
    public Optional<DiscordVideoQualityMode?> VideoQualityMode { get; init; }

    /// <inheritdoc/>
    public Optional<int?> DefaultAutoArchiveDuration { get; init; }

    /// <inheritdoc/>
    public Optional<IDefaultReaction?> DefaultReactionEmoji { get; init; }

    /// <inheritdoc/>
    public Optional<IReadOnlyList<IForumTag>?> AvailableTags { get; init; }

    /// <inheritdoc/>
    public Optional<DiscordForumSortOrder?> DefaultSortOrder { get; init; }

    /// <inheritdoc/>
    public Optional<DiscordForumLayoutType?> DefaultForumLayout { get; init; }

    /// <inheritdoc/>
    public Optional<int?> DefaultThreadRateLimitPerUser { get; init; }
}