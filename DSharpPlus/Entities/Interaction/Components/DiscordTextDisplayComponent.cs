using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents a block of text.
/// </summary>
public sealed class DiscordTextDisplayComponent : DiscordComponent
{
    
    [JsonProperty("content")]
    public string Content { get; internal set; }

    /// <inheritdoc cref="DiscordComponent.Type"/>
    
    public DiscordTextDisplayComponent(string content, int id = default)
    {
        this.Content = content;
        this.Id = id;
    }
    
    internal DiscordTextDisplayComponent() => this.Type = DiscordComponentType.TextDisplay;
}
