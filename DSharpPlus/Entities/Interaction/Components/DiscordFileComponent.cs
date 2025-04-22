using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents a component that will display a single file.
/// </summary>
public sealed class DiscordFileComponent : DiscordComponent
{
    /// <summary>
    /// Gets the file associated with this component. It may be an arbitrary URL or an attachment:// reference.
    /// </summary>
    [JsonProperty("file", NullValueHandling = NullValueHandling.Ignore)]    
    public DiscordUnfurledMediaItem File { get; internal set; }

    /// <summary>
    /// Gets whether this file is spoilered.
    /// </summary>
    [JsonProperty("spoiler")]
    public bool IsSpoilered { get; internal set; }

    public DiscordFileComponent(string url, bool isSpoilered)
        : this()
    {
        this.File = new(url);
        this.IsSpoilered = isSpoilered;
    }

    internal DiscordFileComponent() => this.Type = DiscordComponentType.File;

}
