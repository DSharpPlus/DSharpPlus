// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Remora.Rest.Core;

using DSharpPlus.Core.Abstractions.Models;

namespace DSharpPlus.Core.Models;

/// <inheritdoc cref="IEmbedImage" />
public sealed record EmbedImage : IEmbedImage
{
    /// <inheritdoc/>
    public required string Url { get; init; }

    /// <inheritdoc/>
    public Optional<string> ProxyUrl { get; init; }

    /// <inheritdoc/>
    public Optional<int> Height { get; init; }

    /// <inheritdoc/>
    public Optional<int> Width { get; init; }
}