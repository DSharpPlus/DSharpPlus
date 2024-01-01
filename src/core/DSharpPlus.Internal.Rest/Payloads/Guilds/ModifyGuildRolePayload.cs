// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Entities;
using DSharpPlus.Internal.Abstractions.Rest.Payloads;

namespace DSharpPlus.Internal.Rest.Payloads;

/// <inheritdoc cref="IModifyGuildRolePayload" />
public sealed record ModifyGuildRolePayload : IModifyGuildRolePayload
{
    /// <inheritdoc/>
    public Optional<string?> Name { get; init; }

    /// <inheritdoc/>
    public Optional<DiscordPermissions?> Permissions { get; init; }

    /// <inheritdoc/>
    public Optional<int?> Color { get; init; }

    /// <inheritdoc/>
    public Optional<bool?> Hoist { get; init; }

    /// <inheritdoc/>
    public Optional<ImageData?> Icon { get; init; }

    /// <inheritdoc/>
    public Optional<string?> UnicodeEmoji { get; init; }

    /// <inheritdoc/>
    public Optional<bool?> Mentionable { get; init; }
}