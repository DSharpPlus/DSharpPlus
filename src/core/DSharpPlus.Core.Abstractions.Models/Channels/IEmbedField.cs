// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Core.Abstractions.Models;

/// <summary>
/// Represents an embed field.
/// </summary>
public interface IEmbedField
{
    /// <summary>
    /// The name of the field. This does not support markdown.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The value of this field.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Indicates whether this field is rendered inline.
    /// </summary>
    public Optional<bool> Inline { get; }
}
