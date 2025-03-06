using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents a block of text.
/// </summary>
public sealed class DiscordTextDisplayComponent : DiscordComponent
{
    /// <summary>
    /// Gets the content for this text display. This can be up to 4000 characters, summed by all text displays in a message.
    /// <br/>
    /// One text display could contain 4000 characters, or 10 displays of 400 characters each for example.
    /// </summary>
    [JsonProperty("content")]
    public string Content { get; internal set; }
    
    public DiscordTextDisplayComponent(string content, int id = default)
        : this()
    {
        this.Content = content;
        this.Id = id;
    }
    
    internal DiscordTextDisplayComponent() => this.Type = DiscordComponentType.TextDisplay;
}
