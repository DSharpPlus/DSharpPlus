// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;

using DSharpPlus.Entities;
using DSharpPlus.Internal.Abstractions.Models;
using DSharpPlus.Internal.Abstractions.Rest.Payloads;

namespace DSharpPlus.Internal.Rest.Payloads;

/// <inheritdoc cref="ICreateGuildScheduledEventPayload" />
public sealed record CreateGuildScheduledEventPayload : ICreateGuildScheduledEventPayload
{
    /// <inheritdoc/>
    public Optional<Snowflake> ChannelId { get; init; }

    /// <inheritdoc/>
    public Optional<IScheduledEventMetadata> EntityMetadata { get; init; }

    /// <inheritdoc/>
    public required string Name { get; init; }

    /// <inheritdoc/>
    public required DiscordScheduledEventPrivacyLevel PrivacyLevel { get; init; }

    /// <inheritdoc/>
    public required DateTimeOffset ScheduledStartTime { get; init; }

    /// <inheritdoc/>
    public Optional<DateTimeOffset> ScheduledEndTime { get; init; }

    /// <inheritdoc/>
    public Optional<string> Description { get; init; }

    /// <inheritdoc/>
    public required DiscordScheduledEventType EntityType { get; init; }

    /// <inheritdoc/>
    public Optional<ImageData> Image { get; init; }
}
