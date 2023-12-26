// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Core.Abstractions.Rest.Payloads;

/// <summary>
/// Represents a payload to <c>PATCH /users/@me</c>.
/// </summary>
public interface IModifyCurrentUserPayload
{
    /// <summary>
    /// The new username. If the current user is a bot user, it participates in discriminators, and
    /// changing the username may cause the discriminator to be randomized if the current discriminator
    /// is unavailable for the new username.
    /// </summary>
    public Optional<string> Username { get; }

    /// <summary>
    /// The new avatar for this user.
    /// </summary>
    public Optional<ImageData?> Avatar { get; }
}
