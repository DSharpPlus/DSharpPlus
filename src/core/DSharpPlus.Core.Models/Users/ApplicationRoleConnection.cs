// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Core.Abstractions.Models;

namespace DSharpPlus.Core.Models;

/// <inheritdoc cref="IApplicationRoleConnection" />
public sealed record ApplicationRoleConnection : IApplicationRoleConnection
{
    /// <inheritdoc/>
    public string? PlatformName { get; init; }

    /// <inheritdoc/>
    public string? PlatformUsername { get; init; }

    /// <inheritdoc/>
    public required IReadOnlyDictionary<string, string> Metadata { get; init; }
}