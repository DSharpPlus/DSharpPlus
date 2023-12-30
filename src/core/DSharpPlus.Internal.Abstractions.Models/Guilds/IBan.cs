// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Internal.Abstractions.Models;

/// <summary>
/// Represents information about a ban from a guild.
/// </summary>
public interface IBan
{
    /// <summary>
    /// The ban reason.
    /// </summary>
    public string? Reason { get; }

    /// <summary>
    /// The banned user.
    /// </summary>
    public IUser User { get; }
}
