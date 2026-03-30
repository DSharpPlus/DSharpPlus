using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Fired when a guild soundboard sound is deleted.
/// </summary>
public class GuildSoundboardSoundDeletedEventArgs : DiscordEventArgs
{
    /// <summary>
    /// The guild the soundboard sound belonged to.
    /// </summary>
    public DiscordGuild Guild { get; internal set; }

    /// <summary>
    /// The ID of the soundboard sound that was deleted.
    /// </summary>
    public ulong SoundId { get; internal set; }

    internal GuildSoundboardSoundDeletedEventArgs() : base() { }
}
