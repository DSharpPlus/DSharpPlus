// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Entities;

/// <summary>
/// Represents the type of a message reference.
/// </summary>
public enum DiscordMessageReferenceType
{
    /// <summary>
    /// A standard reference used by replies.
    /// </summary>
    Default,

    /// <summary>
    /// A reference used to point to a message at a certain point in time.
    /// </summary>
    Forward
}
