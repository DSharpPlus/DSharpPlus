// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Internal.Abstractions.Models;

/// <summary>
/// Represents a playable soundboard sound.
/// </summary>
public interface IPartialSoundboardSound
{
    /// <summary>
    /// The snowflake identifier of this sound.
    /// </summary>
    public Snowflake SoundId { get; }

    /// <summary>
    /// The user-readable name of this sound.
    /// </summary>
    public Optional<string> Name { get; }

    /// <summary>
    /// The volume of this sound, in fractions from 0 to 1.
    /// </summary>
    public Optional<double> Volume { get; }

    /// <summary>
    /// The snowflake identifier of this sound's associated custom emoji. Mutually exclusive with <see cref="EmojiName"/>.
    /// </summary>
    public Optional<Snowflake?> EmojiId { get; }

    /// <summary>
    /// The name of this sound's associated unicode emoji. Mutually exclusive with <see cref="EmojiId"/>.
    /// </summary>
    public Optional<string?> EmojiName { get; }

    /// <summary>
    /// The snowflake identifier of the guild this sound lives in. This may be missing for global soundboard sounds.
    /// </summary>
    public Optional<Snowflake> GuildId { get; }

    /// <summary>
    /// Indicates whether this sound is currently available for use.
    /// </summary>
    public Optional<bool> Available { get; }

    /// <summary>
    /// The user who uploaded this sound, if applicable.
    /// </summary>
    public Optional<IUser> User { get; }
}
