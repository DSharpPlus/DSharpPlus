// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;

using DSharpPlus.Internal.Abstractions.Models;

namespace DSharpPlus.Internal.Models;

/// <inheritdoc cref="IVoiceState" />
public sealed record VoiceState : IVoiceState
{
    /// <inheritdoc/>
    public Optional<Snowflake> GuildId { get; init; }

    /// <inheritdoc/>
    public Snowflake? ChannelId { get; init; }

    /// <inheritdoc/>
    public required Snowflake UserId { get; init; }

    /// <inheritdoc/>
    public Optional<IGuildMember> Member { get; init; }

    /// <inheritdoc/>
    public required string SessionId { get; init; }

    /// <inheritdoc/>
    public required bool Deaf { get; init; }

    /// <inheritdoc/>
    public required bool Mute { get; init; }

    /// <inheritdoc/>
    public required bool SelfDeaf { get; init; }

    /// <inheritdoc/>
    public required bool SelfMute { get; init; }

    /// <inheritdoc/>
    public Optional<bool> SelfStream { get; init; }

    /// <inheritdoc/>
    public required bool SelfVideo { get; init; }

    /// <inheritdoc/>
    public required bool Suppress { get; init; }

    /// <inheritdoc/>
    public DateTimeOffset? RequestToSpeakTimestamp { get; init; }
}
