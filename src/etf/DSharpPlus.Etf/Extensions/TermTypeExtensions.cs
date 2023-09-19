// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Etf.Extensions;

/// <summary>
/// Contains extensions on <seealso cref="TermType"/> to reason about the .NET representation
/// of a given term.
/// </summary>
public static class TermTypeExtensions
{
    /// <summary>
    /// Determines whether this term can be represented in .NET as a string.
    /// </summary>
    public static bool IsString
    (
        this TermType term
    )
        => term is TermType.AtomUtf8 or TermType.SmallAtomUtf8 or TermType.String;

    /// <summary>
    /// Determines whether this term is an integer.
    /// </summary>
    public static bool IsInteger
    (
        this TermType term
    )
        => term is TermType.SmallBig or TermType.LargeBig or TermType.SmallInteger or TermType.Integer;

    /// <summary>
    /// Determines whether this term is a big integer.
    /// </summary>
    public static bool IsBigInteger
    (
        this TermType term
    )
        => term is TermType.SmallBig or TermType.LargeBig;

    /// <summary>
    /// Determines whether this term is a tuple.
    /// </summary>
    public static bool IsTuple
    (
        this TermType term
    )
        => term is TermType.SmallTuple or TermType.LargeTuple;

    /// <summary>
    /// Determines whether this term is a float.
    /// </summary>
    public static bool IsFloat
    (
        this TermType term
    )
        => term is TermType.Float or TermType.NewFloat;
}
