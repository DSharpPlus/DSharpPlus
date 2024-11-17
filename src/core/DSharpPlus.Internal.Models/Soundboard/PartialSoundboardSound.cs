// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Internal.Abstractions.Models;

namespace DSharpPlus.Internal.Models;

/// <inheritdoc cref="IPartialSoundboardSound" />
public sealed record PartialSoundboardSound : IPartialSoundboardSound
{
    /// <inheritdoc/>
    public required Snowflake SoundId { get; init; }

    /// <inheritdoc/>
    public Optional<string> Name { get; init; }

    /// <inheritdoc/>
    public Optional<double> Volume { get; init; }

    /// <inheritdoc/>
    public Optional<Snowflake?> EmojiId { get; init; }

    /// <inheritdoc/>
    public Optional<string?> EmojiName { get; init; }

    /// <inheritdoc/>
    public Optional<Snowflake> GuildId { get; init; }

    /// <inheritdoc/>
    public Optional<bool> Available { get; init; }

    /// <inheritdoc/>
    public Optional<IUser> User { get; init; }
}