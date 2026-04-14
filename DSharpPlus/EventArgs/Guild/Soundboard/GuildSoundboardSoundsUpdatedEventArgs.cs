using System.Collections.Generic;
using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Fired when multiple guild soundboard sounds are updated.
/// </summary>
public class GuildSoundboardSoundsUpdatedEventArgs : DiscordEventArgs
{
    /// <summary>
    /// The guild the soundboard sounds belong to.
    /// </summary>
    public DiscordGuild Guild { get; internal set; }

    /// <summary>
    /// The updated soundboard sounds.
    /// </summary>
    public IReadOnlyList<DiscordSoundboardSound> SoundboardSounds { get; internal set; }

    internal GuildSoundboardSoundsUpdatedEventArgs() : base() { }
}
