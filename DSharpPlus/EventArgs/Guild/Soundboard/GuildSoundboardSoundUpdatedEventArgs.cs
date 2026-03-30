using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Fired when a guild soundboard sound is updated.
/// </summary>
public class GuildSoundboardSoundUpdatedEventArgs : DiscordEventArgs
{
    /// <summary>
    /// The guild the soundboard sound belongs to.
    /// </summary>
    public DiscordGuild Guild { get; internal set; }

    /// <summary>
    /// The soundboard sound before the update, or null if it wasn't cached.
    /// </summary>
    public DiscordSoundboardSound SoundboardSoundBefore { get; internal set; }

    /// <summary>
    /// The soundboard sound after the update.
    /// </summary>
    public DiscordSoundboardSound SoundboardSoundAfter { get; internal set; }

    internal GuildSoundboardSoundUpdatedEventArgs() : base() { }
}
