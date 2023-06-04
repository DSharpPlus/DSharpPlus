// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Remora.Rest.Core;

namespace DSharpPlus.Core.Abstractions.Models;

/// <summary>
/// Represents an embed provider.
/// </summary>
public interface IEmbedProvider
{
    /// <summary>
    /// The name of this provider.
    /// </summary>
    public Optional<string> Name { get; }

    /// <summary>
    /// The URL of this provider.
    /// </summary>
    public Optional<string> Url { get; }
}
