// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Entities;
using DSharpPlus.Internal.Abstractions.Models;

namespace DSharpPlus.Internal.Models;

/// <inheritdoc cref="IPartialStageInstance" />
public sealed record PartialStageInstance : IPartialStageInstance
{
    /// <inheritdoc/>
    public Optional<Snowflake> Id { get; init; }

    /// <inheritdoc/>
    public Optional<Snowflake> GuildId { get; init; }

    /// <inheritdoc/>
    public Optional<Snowflake> ChannelId { get; init; }

    /// <inheritdoc/>
    public Optional<string> Topic { get; init; }

    /// <inheritdoc/>
    public Optional<DiscordStagePrivacyLevel> PrivacyLevel { get; init; }

    /// <inheritdoc/>
    public Optional<Snowflake?> GuildScheduledEventId { get; init; }
}