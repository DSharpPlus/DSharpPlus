// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Core.Abstractions.Models;

namespace DSharpPlus.Core.Models;

/// <inheritdoc cref="IWelcomeScreen" />
public sealed record WelcomeScreen : IWelcomeScreen
{
    /// <inheritdoc/>
    public string? Description { get; init; }

    /// <inheritdoc/>
    public required IReadOnlyList<IWelcomeScreenChannel> WelcomeChannels { get; init; }
}