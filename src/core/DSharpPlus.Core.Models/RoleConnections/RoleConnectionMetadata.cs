// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Entities;

using Remora.Rest.Core;

using DSharpPlus.Core.Abstractions.Models;

namespace DSharpPlus.Core.Models;

/// <inheritdoc cref="IRoleConnectionMetadata" />
public sealed record RoleConnectionMetadata : IRoleConnectionMetadata
{
    /// <inheritdoc/>
    public required DiscordRoleConnectionMetadataType Type { get; init; }

    /// <inheritdoc/>
    public required string Key { get; init; }

    /// <inheritdoc/>
    public required string Name { get; init; }

    /// <inheritdoc/>
    public Optional<IReadOnlyDictionary<string, string>?> NameLocalizations { get; init; }

    /// <inheritdoc/>
    public required string Description { get; init; }

    /// <inheritdoc/>
    public Optional<IReadOnlyDictionary<string, string>?> DescriptionLocalizations { get; init; }
}