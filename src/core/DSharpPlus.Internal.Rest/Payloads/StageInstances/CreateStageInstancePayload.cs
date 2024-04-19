// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Entities;
using DSharpPlus.Internal.Abstractions.Rest.Payloads;

namespace DSharpPlus.Internal.Rest.Payloads;

/// <inheritdoc cref="ICreateStageInstancePayload" />
public sealed record CreateStageInstancePayload : ICreateStageInstancePayload
{
    /// <inheritdoc/>
    public required Snowflake ChannelId { get; init; }

    /// <inheritdoc/>
    public required string Topic { get; init; }

    /// <inheritdoc/>
    public Optional<DiscordStagePrivacyLevel> PrivacyLevel { get; init; }

    /// <inheritdoc/>
    public Optional<bool> SendStartNotification { get; init; }

    /// <inheritdoc/>
    public Optional<Snowflake> GuildScheduledEventId { get; init; }
}
