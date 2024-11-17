// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Internal.Abstractions.Rest.Payloads;

/// <summary>
/// Represents a payload to <c>POST guilds/:guild-id/soundboard-sounds</c>.
/// </summary>
public interface ICreateGuildSoundboardSoundPayload
{
    /// <summary>
    /// The name of the soundboard sound, 2-32 characters long.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The sound data. This must be mp3 or ogg encoded.
    /// </summary>
    public InlineMediaData Sound { get; }

    /// <summary>
    /// The volume of this soundboard sound, from 0 to 1. Defaults to 1.
    /// </summary>
    public Optional<double?> Volume { get; }

    /// <summary>
    /// The snowflake identifier of the custom emoji associated with this soundboard sound.
    /// </summary>
    public Optional<Snowflake?> EmojiId { get; }

    /// <summary>
    /// The unicode representation of the standard emoji associated with this soundboard sound.
    /// </summary>
    public Optional<string?> EmojiName { get; }
}
