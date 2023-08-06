// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Entities;

using Remora.Rest.Core;

using DSharpPlus.Core.Abstractions.Models;

namespace DSharpPlus.Core.Models;

/// <inheritdoc cref="IStageInstance" />
public sealed record StageInstance : IStageInstance
{
    /// <inheritdoc/>
    public required Snowflake Id { get; init; }

    /// <inheritdoc/>
    public required Snowflake GuildId { get; init; }

    /// <inheritdoc/>
    public required Snowflake ChannelId { get; init; }

    /// <inheritdoc/>
    public required string Topic { get; init; }

    /// <inheritdoc/>
    public required DiscordStagePrivacyLevel PrivacyLevel { get; init; }

    /// <inheritdoc/>
    public required Snowflake? GuildScheduledEventId { get; init; }
}