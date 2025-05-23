// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Internal.Abstractions.Rest.Payloads;

namespace DSharpPlus.Internal.Rest.Payloads;

/// <inheritdoc cref="ISendSoundboardSoundPayload" />
public sealed record SendSoundboardSoundPayload : ISendSoundboardSoundPayload
{
    /// <inheritdoc/>
    public required Snowflake SoundId { get; init; }

    /// <inheritdoc/>
    public Optional<Snowflake> SourceGuildId { get; init; }
}