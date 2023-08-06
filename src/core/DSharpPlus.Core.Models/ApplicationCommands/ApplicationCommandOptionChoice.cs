// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using OneOf;

using Remora.Rest.Core;

using DSharpPlus.Core.Abstractions.Models;

namespace DSharpPlus.Core.Models;

/// <inheritdoc cref="IApplicationCommandOptionChoice" />
public sealed record ApplicationCommandOptionChoice : IApplicationCommandOptionChoice
{
    /// <inheritdoc/>
    public required string Name { get; init; }

    /// <inheritdoc/>
    public Optional<IReadOnlyDictionary<string, string>?> NameLocalizations { get; init; }

    /// <inheritdoc/>
    public required OneOf<string, int, double> Value { get; init; }
}