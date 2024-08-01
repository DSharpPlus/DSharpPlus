// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Internal.Abstractions.Models;

/// <summary>
/// Represents a snapshot taken of a message.
/// </summary>
public interface IMessageSnapshot
{
    /// <summary>
    /// A subset of the data contained within the referenced message.
    /// </summary>
    public IPartialMessage Message { get; }
}
