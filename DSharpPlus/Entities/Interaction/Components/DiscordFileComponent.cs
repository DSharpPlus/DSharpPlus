using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents a component that will display a single file.
/// </summary>
public sealed class DiscordFileComponent : DiscordComponent
{
        
    public DiscordUnfurledMediaItem File { get; internal set; }

    [JsonProperty("spoilered")]
    public bool IsSpoilered { get; internal set; }

    public DiscordFileComponent(string url, bool isSpoilered)
    {
        this.File = new(url);
        this.IsSpoilered = isSpoilered;
    }

}
