// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Internal.Abstractions.Models;

/// <summary>
/// Represents an embed footer object.
/// </summary>
public interface IEmbedFooter
{
    /// <summary>
    /// The footer text.
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// The URL of the footer icon.
    /// </summary>
    public Optional<string> IconUrl { get; }

    /// <summary>
    /// The proxied URL of the footer icon.
    /// </summary>
    public Optional<string> ProxyIconUrl { get; }
}
