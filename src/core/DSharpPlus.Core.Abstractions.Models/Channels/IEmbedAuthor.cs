// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Core.Abstractions.Models;

/// <summary>
/// Represents an embed author object.
/// </summary>
public interface IEmbedAuthor
{
    /// <summary>
    /// The name of the author.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The URL of the author, only supports http(s).
    /// </summary>
    public Optional<string> Url { get; }

    /// <summary>
    /// The URL of the author icon.
    /// </summary>
    public Optional<string> IconUrl { get; }

    /// <summary>
    /// The proxied URL of the author icon.
    /// </summary>
    public Optional<string> ProxyIconUrl { get; }
}
