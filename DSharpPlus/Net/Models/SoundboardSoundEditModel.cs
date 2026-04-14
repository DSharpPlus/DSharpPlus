using DSharpPlus.Entities;

namespace DSharpPlus.Net.Models;

public class SoundboardSoundEditModel : BaseEditModel
{
    /// <summary>
    /// Sets the soundboard sound's new name.
    /// </summary>
    public string Name { internal get; set; }

    /// <summary>
    /// Sets the soundboard sound's new volume.
    /// </summary>
    public Optional<double>? Volume { internal get; set; }

    /// <summary>
    /// Sets the soundboard sound's new emoji.
    /// </summary>
    public Optional<DiscordEmoji> Emoji { internal get; set; }

    internal SoundboardSoundEditModel() { }
}