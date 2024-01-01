// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Entities;
using DSharpPlus.Internal.Abstractions.Rest.Payloads;

namespace DSharpPlus.Internal.Rest.Payloads;

/// <inheritdoc cref="IEditGlobalApplicationCommandPayload" />
public sealed record EditGlobalApplicationCommandPayload : IEditGlobalApplicationCommandPayload
{
    /// <inheritdoc/>
    public Optional<string> Name { get; init; }

    /// <inheritdoc/>
    public Optional<IReadOnlyDictionary<string, string>?> NameLocalizations { get; init; }

    /// <inheritdoc/>
    public Optional<string> Description { get; init; }

    /// <inheritdoc/>
    public Optional<IReadOnlyDictionary<string, string>?> DescriptionLocalizations { get; init; }

    /// <inheritdoc/>
    public Optional<DiscordPermissions?> DefaultMemberPermissions { get; init; }

    /// <inheritdoc/>
    public Optional<bool?> DmPermission { get; init; }

    /// <inheritdoc/>
    public Optional<DiscordApplicationCommandType> Type { get; init; }

    /// <inheritdoc/>
    public Optional<bool> Nsfw { get; init; }
}