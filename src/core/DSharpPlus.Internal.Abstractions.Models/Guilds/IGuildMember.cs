// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;

using DSharpPlus.Entities;

namespace DSharpPlus.Internal.Abstractions.Models;

/// <summary>
/// Represents a fully populated guild member object.
/// </summary>
public interface IGuildMember : IPartialGuildMember
{
    /// <inheritdoc cref="IPartialGuildMember.Roles"/>
    public new IReadOnlyList<Snowflake> Roles { get; }

    /// <inheritdoc cref="IPartialGuildMember.JoinedAt"/>
    public new DateTimeOffset JoinedAt { get; }

    /// <inheritdoc cref="IPartialGuildMember.Deaf"/>
    public new bool Deaf { get; }

    /// <inheritdoc cref="IPartialGuildMember.Mute"/>
    public new bool Mute { get; }

    /// <inheritdoc cref="IPartialGuildMember.Flags"/>
    public new DiscordGuildMemberFlags Flags { get; }

    // explicit routes for partial guild member access

    /// <inheritdoc/>
    Optional<IReadOnlyList<Snowflake>> IPartialGuildMember.Roles => new(this.Roles);

    /// <inheritdoc/>
    Optional<DateTimeOffset> IPartialGuildMember.JoinedAt => this.JoinedAt;

    /// <inheritdoc/>
    Optional<bool> IPartialGuildMember.Deaf => this.Deaf;

    /// <inheritdoc/>
    Optional<bool> IPartialGuildMember.Mute => this.Mute;

    /// <inheritdoc/>
    Optional<DiscordGuildMemberFlags> IPartialGuildMember.Flags => this.Flags;
}
