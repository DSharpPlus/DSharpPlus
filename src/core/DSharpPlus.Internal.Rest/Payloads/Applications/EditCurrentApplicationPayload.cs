// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Entities;
using DSharpPlus.Internal.Abstractions.Models;
using DSharpPlus.Internal.Abstractions.Rest.Payloads;

namespace DSharpPlus.Internal.Rest.Payloads;

/// <inheritdoc cref="IEditCurrentApplicationPayload" />
public sealed record EditCurrentApplicationPayload : IEditCurrentApplicationPayload
{
    /// <inheritdoc/>
    public Optional<string> CustomInstallUrl { get; init; }

    /// <inheritdoc/>
    public Optional<string> Description { get; init; }

    /// <inheritdoc/>
    public Optional<string> RoleConnectionsVerificationUrl { get; init; }

    /// <inheritdoc/>
    public Optional<IInstallParameters> InstallParams { get; init; }

    /// <inheritdoc/>
    public Optional<DiscordApplicationFlags> Flags { get; init; }

    /// <inheritdoc/>
    public Optional<ImageData?> Icon { get; init; }

    /// <inheritdoc/>
    public Optional<ImageData?> CoverImage { get; init; }

    /// <inheritdoc/>
    public Optional<string> InteractionsEndpointUrl { get; init; }

    /// <inheritdoc/>
    public Optional<IReadOnlyList<string>> Tags { get; init; }
}