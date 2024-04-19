// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Internal.Abstractions.Models;

/// <summary>
/// The backing text field object for polls. Questions can only contain <seealso cref="Text"/>, while answers can also contain
/// <seealso cref="Emoji"/>.
/// </summary>
public interface IPollMedia
{
    /// <summary>
    /// The contents of the text field.
    /// </summary>
    public Optional<string> Text { get; }

    /// <summary>
    /// An optional emoji attached to poll answers.
    /// </summary>
    public Optional<IPartialEmoji> Emoji { get; }
}
