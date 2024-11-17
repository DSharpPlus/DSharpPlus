// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Internal.Abstractions.Models;

/// <inheritdoc cref="IPartialSoundboardSound"/>
public interface ISoundboardSound : IPartialSoundboardSound
{
    /// <inheritdoc cref="IPartialSoundboardSound.Name"/>
    public new string Name { get; }

    /// <inheritdoc cref="IPartialSoundboardSound.Volume"/>
    public new double Volume { get; }

    /// <inheritdoc cref="IPartialSoundboardSound.EmojiId"/>
    public new Snowflake? EmojiId { get; }

    /// <inheritdoc cref="IPartialSoundboardSound.EmojiName"/>
    public new string? EmojiName { get; }

    /// <inheritdoc cref="IPartialSoundboardSound.Available"/>
    public new bool Available { get; }

    // partial access

    /// <inheritdoc/>
    Optional<string> IPartialSoundboardSound.Name => Name;

    /// <inheritdoc/>
    Optional<double> IPartialSoundboardSound.Volume => Volume;

    /// <inheritdoc/>
    Optional<Snowflake?> IPartialSoundboardSound.EmojiId => EmojiId;

    /// <inheritdoc/>
    Optional<string?> IPartialSoundboardSound.EmojiName => EmojiName;

    /// <inheritdoc/>
    Optional<bool> IPartialSoundboardSound.Available => Available;
}
