using System.Collections.Generic;
using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Fired in response to Request Soundboard Sounds, containing a guild's soundboard sounds.
/// </summary>
public class SoundboardSoundsReceivedEventArgs : DiscordEventArgs
{
    /// <summary>
    /// The ID of the guild the soundboard sounds belong to.
    /// </summary>
    public ulong GuildId { get; internal set; }

    /// <summary>
    /// The soundboard sounds for the guild.
    /// </summary>
    public IReadOnlyList<DiscordSoundboardSound> SoundboardSounds { get; internal set; }

    internal SoundboardSoundsReceivedEventArgs() : base() { }
}
