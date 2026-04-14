using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Fired when a guild soundboard sound is created.
/// </summary>
public class GuildSoundboardSoundCreatedEventArgs : DiscordEventArgs
{
    /// <summary>
    /// The guild the soundboard sound belongs to.
    /// </summary>
    public DiscordGuild Guild { get; internal set; }

    /// <summary>
    /// The soundboard sound that was created.
    /// </summary>
    public DiscordSoundboardSound SoundboardSound { get; internal set; }

    internal GuildSoundboardSoundCreatedEventArgs() : base() { }
}
