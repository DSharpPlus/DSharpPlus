// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Remora.Rest.Core;

namespace DSharpPlus.Core.Abstractions.Models;

/// <summary>
/// Represents a discord invite object.
/// </summary>
public interface IInvite : IPartialInvite
{
    /// <inheritdoc cref="IPartialInvite.Code"/>
    public new string Code { get; }

    /// <inheritdoc cref="IPartialInvite.Channel"/>
    public new IPartialChannel? Channel { get; }

    // partial access routes

    /// <inheritdoc/>
    Optional<string> IPartialInvite.Code => this.Code;

    /// <inheritdoc/>
    Optional<IPartialChannel?> IPartialInvite.Channel => new(this.Channel);
}
