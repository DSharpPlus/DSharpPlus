// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Entities;

/// <summary>
/// The specific kinds of metadata comparisons that can be made.
/// </summary>
public enum DiscordRoleConnectionMetadataType
{
    /// <summary>
    /// The metadata integer is less than or equal to the guild's configured value.
    /// </summary>
    IntegerLessThanOrEqual = 1,

    /// <summary>
    /// The metadata integer is greater than or equal to the guild's configured value.
    /// </summary>
    IntegerGreaterThanOrEqual,

    /// <summary>
    /// The metadata integer is equal to the guild's configured value.
    /// </summary>
    IntegerEqual,

    /// <summary>
    /// The metadata integer is not equal to the guild's configured value.
    /// </summary>
    IntegerNotEqual,

    /// <summary>
    /// The metadata date/time object is less than or equal to the guild's configured value.
    /// </summary>
    DateTimeLessThanOrEqual,

    /// <summary>
    /// The metadata date/time object is greater than or equal to the guild's configured value.
    /// </summary>
    DateTimeGreaterThanOrEqual,

    /// <summary>
    /// The metadata boolean is equal to the guild's configured value.
    /// </summary>
    BooleanEqual,

    /// <summary>
    /// The metadata boolean is not equal to the guild's configured value.
    /// </summary>
    BooleanNotEqual
}
