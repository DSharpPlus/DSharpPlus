// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;

using DSharpPlus.Entities;
using DSharpPlus.Internal.Abstractions.Models;

namespace DSharpPlus.Internal.Models;

/// <inheritdoc cref="IGuildMember" />
public sealed record GuildMember : IGuildMember
{
    /// <inheritdoc/>
    public Optional<IUser> User { get; init; }

    /// <inheritdoc/>
    public Optional<string?> Nick { get; init; }

    /// <inheritdoc/>
    public Optional<string?> Avatar { get; init; }

    /// <inheritdoc/>
    public Optional<string?> Banner { get; init; }

    /// <inheritdoc/>
    public required IReadOnlyList<Snowflake> Roles { get; init; }

    /// <inheritdoc/>
    public required DateTimeOffset JoinedAt { get; init; }

    /// <inheritdoc/>
    public Optional<DateTimeOffset?> PremiumSince { get; init; }

    /// <inheritdoc/>
    public required bool Deaf { get; init; }

    /// <inheritdoc/>
    public required bool Mute { get; init; }

    /// <inheritdoc/>
    public required DiscordGuildMemberFlags Flags { get; init; }

    /// <inheritdoc/>
    public Optional<bool> Pending { get; init; }

    /// <inheritdoc/>
    public Optional<DiscordPermissions> Permissions { get; init; }

    /// <inheritdoc/>
    public Optional<DateTimeOffset?> CommunicationDisabledUntil { get; init; }

    /// <inheritdoc/>
    public Optional<IAvatarDecorationData?> AvatarDecorationData { get; init; }
}