// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;

using DSharpPlus.Internal.Abstractions.Rest.Payloads;

namespace DSharpPlus.Internal.Rest.Payloads;

/// <inheritdoc cref="IModifyCurrentUserVoiceStatePayload" />
public sealed record ModifyCurrentUserVoiceStatePayload : IModifyCurrentUserVoiceStatePayload
{
    /// <inheritdoc/>
    public Optional<Snowflake> ChannelId { get; init; }

    /// <inheritdoc/>
    public Optional<bool> Suppress { get; init; }

    /// <inheritdoc/>
    public Optional<DateTimeOffset?> RequestToSpeakTimestamp { get; init; }
}