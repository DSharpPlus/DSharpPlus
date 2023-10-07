// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus;

/// <summary>
/// Defines the principal working of an Optional type.
/// </summary>
public interface IOptional
{
    /// <summary>
    /// Indicates whether this optional is logically defined.
    /// </summary>
    public bool HasValue { get; }
}
