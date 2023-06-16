// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

namespace DSharpPlus.Core.Abstractions.Models;

/// <summary>
/// Represents a guild welcome screen, showing the user a brief description and a few channels to check out.
/// </summary>
public interface IWelcomeScreen
{
    /// <summary>
    /// The server description as shown in the welcome screen.
    /// </summary>
    public string? Description { get; }

    /// <summary>
    /// The channels shown in the welcome screen, up to five.
    /// </summary>
    public IReadOnlyList<IWelcomeScreenChannel> WelcomeChannels { get; }
}
