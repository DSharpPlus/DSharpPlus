// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Entities;

namespace DSharpPlus.Internal.Abstractions.Models;

/// <summary>
/// Represents a discord invite object.
/// </summary>
public interface IInvite : IPartialInvite
{
    /// <inheritdoc cref="IPartialInvite.Type"/>
    public new DiscordInviteType Type { get; }

    /// <inheritdoc cref="IPartialInvite.Code"/>
    public new string Code { get; }

    /// <inheritdoc cref="IPartialInvite.Channel"/>
    public new IPartialChannel? Channel { get; }

    // partial access routes

    /// <inheritdoc/>
    Optional<DiscordInviteType> IPartialInvite.Type => this.Type;

    /// <inheritdoc/>
    Optional<string> IPartialInvite.Code => this.Code;

    /// <inheritdoc/>
    Optional<IPartialChannel?> IPartialInvite.Channel => new(this.Channel);
}
