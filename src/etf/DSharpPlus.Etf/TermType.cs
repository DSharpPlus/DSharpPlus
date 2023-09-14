// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Etf;

/// <summary>
/// Specifies the token type prefixes for each type, according to ERTS 14.0.2, format version 131.
/// </summary>
#pragma warning disable CA1008 // Enums should have zero value, the "none" value is 108
public enum TermType : byte
#pragma warning restore CA1008
{
    /// <summary>
    /// erl_ext_dist 4
    /// </summary>
    AtomCache = 82,

    /// <summary>
    /// erl_ext_dist 5
    /// </summary>
    SmallInteger = 97,

    /// <summary>
    /// erl_ext_dist 6
    /// </summary>
    Integer = 98,

    /// <summary>
    /// erl_ext_dist 7
    /// </summary>
    Float = 99,

    /// <summary>
    /// erl_ext_dist 8
    /// </summary>
    Port = 102,

    /// <summary>
    /// erl_ext_dist 9
    /// </summary>
    NewPort = 89,

    /// <summary>
    /// erl_ext_dist 10
    /// </summary>
    V4Port = 120,

    /// <summary>
    /// erl_ext_dist 11
    /// </summary>
    Pid = 103,

    /// <summary>
    /// erl_ext_dist 12
    /// </summary>
    NewPid = 88,

    /// <summary>
    /// erl_ext_dist 13
    /// </summary>
    SmallTuple = 104,

    /// <summary>
    /// erl_ext_dist 14
    /// </summary>
    LargeTuple = 105,

    /// <summary>
    /// erl_ext_dist 15
    /// </summary>
    Map = 116,

    /// <summary>
    /// erl_ext_dist 16
    /// </summary>
    Nil = 106,

    /// <summary>
    /// erl_ext_dist 17
    /// </summary>
    String = 107,

    /// <summary>
    /// erl_ext_dist 18
    /// </summary>
    List = 108,

    /// <summary>
    /// erl_ext_dist 19
    /// </summary>
    Binary = 109,

    /// <summary>
    /// erl_ext_dist 20
    /// </summary>
    SmallBig = 110,

    /// <summary>
    /// erl_ext_dist 21
    /// </summary>
    LargeBig = 111,

    // deprecated
    //
    // /// <summary>
    // /// erl_ext_dist 22
    // /// </summary>
    // Reference = 101,

    /// <summary>
    /// erl_ext_dist 23
    /// </summary>
    NewReference = 114,

    /// <summary>
    /// erl_ext_dist 24
    /// </summary>
    NewerReference = 90,

    // removed
    //
    // /// <summary>
    // /// erl_ext_dist 25
    // /// </summary>
    // Fun = 117,
    //
    // unsupported
    //
    // /// <summary>
    // /// erl_ext_dist 26
    // /// </summary>
    // NewFun = 112,

    /// <summary>
    /// erl_ext_dist 27
    /// </summary>
    Export = 113,

    /// <summary>
    /// erl_ext_dist 28
    /// </summary>
    BitBinary = 77,

    /// <summary>
    /// erl_ext_dist 29
    /// </summary>
    NewFloat = 70,

    /// <summary>
    /// erl_ext_dist 30
    /// </summary>
    AtomUtf8 = 118,

    /// <summary>
    /// erl_ext_dist 31
    /// </summary>
    SmallAtomUtf8 = 119,

    // deprecated
    //
    // /// <summary>
    // /// erl_ext_dist 32
    // /// </summary>
    // Atom = 100,
    //
    // deprecated
    //
    // /// <summary>
    // /// erl_ext_dist 33
    // /// </summary>
    // SmallAtom = 115,

    /// <summary>
    /// erl_ext_dist 34
    /// </summary>
    Local = 121,
}
