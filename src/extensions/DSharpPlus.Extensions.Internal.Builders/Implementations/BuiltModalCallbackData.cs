// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Internal.Abstractions.Models;

namespace DSharpPlus.Extensions.Internal.Builders.Implementations;

/// <inheritdoc cref="IModalCallbackData" />
internal sealed record BuiltModalCallbackData : IModalCallbackData
{
    /// <inheritdoc/>
    public required string CustomId { get; init; }

    /// <inheritdoc/>
    public required string Title { get; init; }

    /// <inheritdoc/>
    public required IReadOnlyList<IActionRowComponent> Components { get; init; }
}