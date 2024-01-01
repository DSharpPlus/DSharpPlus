// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;

using DSharpPlus.Entities;
using DSharpPlus.Internal.Abstractions.Models;
using DSharpPlus.Internal.Abstractions.Rest.Payloads;

namespace DSharpPlus.Internal.Rest.Payloads;

/// <inheritdoc cref="IModifyGuildScheduledEventPayload" />
public sealed record ModifyGuildScheduledEventPayload : IModifyGuildScheduledEventPayload
{
    /// <inheritdoc/>
    public Optional<Snowflake?> ChannelId { get; init; }

    /// <inheritdoc/>
    public Optional<IScheduledEventMetadata?> EntityMetadata { get; init; }

    /// <inheritdoc/>
    public Optional<string> Name { get; init; }

    /// <inheritdoc/>
    public Optional<DiscordScheduledEventPrivacyLevel> PrivacyLevel { get; init; }

    /// <inheritdoc/>
    public Optional<DateTimeOffset> ScheduledStartTime { get; init; }

    /// <inheritdoc/>
    public Optional<DateTimeOffset> ScheduledEndTime { get; init; }

    /// <inheritdoc/>
    public Optional<string?> Description { get; init; }

    /// <inheritdoc/>
    public Optional<DiscordScheduledEventType> EntityType { get; init; }

    /// <inheritdoc/>
    public Optional<DiscordScheduledEventStatus> Status { get; init; }

    /// <inheritdoc/>
    public Optional<ImageData> Image { get; init; }
}