// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Entities;
using DSharpPlus.Internal.Abstractions.Models;
using DSharpPlus.Internal.Abstractions.Rest.Payloads;

namespace DSharpPlus.Internal.Rest.Payloads;

/// <inheritdoc cref="ICreateGuildPayload" />
public sealed record CreateGuildPayload : ICreateGuildPayload
{
    /// <inheritdoc/>
    public required string Name { get; init; }

    /// <inheritdoc/>
    public Optional<ImageData> Icon { get; init; }

    /// <inheritdoc/>
    public Optional<DiscordVerificationLevel> VerificationLevel { get; init; }

    /// <inheritdoc/>
    public Optional<DiscordMessageNotificationLevel> DefaultMessageNotifications { get; init; }

    /// <inheritdoc/>
    public Optional<DiscordExplicitContentFilterLevel> ExplicitContentFilter { get; init; }

    /// <inheritdoc/>
    public Optional<IReadOnlyList<IPartialRole>> Roles { get; init; }

    /// <inheritdoc/>
    public Optional<IReadOnlyList<IPartialChannel>> Channels { get; init; }

    /// <inheritdoc/>
    public Optional<Snowflake> AfkChannelId { get; init; }

    /// <inheritdoc/>
    public Optional<int> AfkTimeout { get; init; }

    /// <inheritdoc/>
    public Optional<Snowflake> SystemChannelId { get; init; }

    /// <inheritdoc/>
    public Optional<DiscordSystemChannelFlags> SystemChannelFlags { get; init; }
}