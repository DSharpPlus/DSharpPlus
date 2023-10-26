// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Core.Abstractions.Models;
using DSharpPlus.Entities;

namespace DSharpPlus.Core.Models;

/// <inheritdoc cref="IApplicationCommand" />
public sealed record ApplicationCommand : IApplicationCommand
{
    /// <inheritdoc/>
    public required Snowflake Id { get; init; }

    /// <inheritdoc/>
    public Optional<DiscordApplicationCommandType> Type { get; init; }

    /// <inheritdoc/>
    public required Snowflake ApplicationId { get; init; }

    /// <inheritdoc/>
    public Optional<Snowflake> GuildId { get; init; }

    /// <inheritdoc/>
    public required string Name { get; init; }

    /// <inheritdoc/>
    public Optional<IReadOnlyDictionary<string, string>?> NameLocalizations { get; init; }

    /// <inheritdoc/>
    public required string Description { get; init; }

    /// <inheritdoc/>
    public Optional<IReadOnlyDictionary<string, string>?> DescriptionLocalizations { get; init; }

    /// <inheritdoc/>
    public Optional<IReadOnlyList<IApplicationCommandOption>> Options { get; init; }

    /// <inheritdoc/>
    public DiscordPermissions? DefaultMemberPermissions { get; init; }

    /// <inheritdoc/>
    public Optional<bool> DmPermission { get; init; }

    /// <inheritdoc/>
    public Optional<bool> Nsfw { get; init; }

    /// <inheritdoc/>
    public required Snowflake Version { get; init; }
}