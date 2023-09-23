// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Etf.Objects;

/// <summary>
/// Represents an Erlang process identifier object.
/// </summary>
public readonly record struct PidTerm
{
    /// <summary>
    /// The originating node.
    /// </summary>
    public required string Node { get; init; }

    /// <summary>
    /// The relevant ID.
    /// </summary>
    public required uint Id { get; init; }

    /// <summary>
    /// The relevant serial number.
    /// </summary>
    public required uint Serial { get; init; }

    /// <summary>
    /// An identifier indicating the incarnation of a node.
    /// </summary>
    public required byte Creation { get; init; }
}
