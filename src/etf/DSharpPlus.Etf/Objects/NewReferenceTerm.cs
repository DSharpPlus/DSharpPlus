// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

namespace DSharpPlus.Etf.Objects;

/// <summary>
/// Represents a new Erlang reference term.
/// </summary>
public readonly record struct NewReferenceTerm
{
    /// <summary>
    /// The originating node.
    /// </summary>
    public required string Node { get; init; }

    /// <summary>
    /// An identifier indicating the incarnation of a node.
    /// </summary>
    public required byte Creation { get; init; }

    /// <summary>
    /// The relevant ID, to be regarded as uninterpreted data.
    /// </summary>
    public required IReadOnlyList<uint> Id { get; init; }
}
