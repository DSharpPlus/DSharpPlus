// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Remora.Rest.Core;

namespace DSharpPlus.Core.Abstractions.Models;

/// <summary>
/// Represents a single text option in a <seealso cref="IStringSelectComponent"/>.
/// </summary>
public interface ISelectOption
{
    /// <summary>
    /// The user-facing name of this option, up to 100 characters.
    /// </summary>
    public string Label { get; }

    /// <summary>
    /// The developer-defined value of this option, up to 100 characters.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// An additional description of this option, up to 100 characters.
    /// </summary>
    public Optional<string> Description { get; }

    /// <summary>
    /// The emoji to render with this option
    /// </summary>
    public Optional<IPartialEmoji> Emoji { get; }

    /// <summary>
    /// Indicates whether this option will be selected by default.
    /// </summary>
    public Optional<bool> Default { get; }
}
