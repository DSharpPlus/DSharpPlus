// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Internal.Abstractions.Rest.Payloads;

/// <summary>
/// Represents a payload to <c>POST channels/:channel-id/send-soundboard-sound</c>.
/// </summary>
public interface ISendSoundboardSoundPayload
{
    /// <summary>
    /// The snowflake identifier of the soundboard sound to play.
    /// </summary>
    public Snowflake SoundId { get; }

    /// <summary>
    /// The snowflake identifier of the guild the soundboard sound originates from.
    /// This is required to play sounds from different guilds to the present guild.
    /// </summary>
    public Optional<Snowflake> SourceGuildId { get; }
}
