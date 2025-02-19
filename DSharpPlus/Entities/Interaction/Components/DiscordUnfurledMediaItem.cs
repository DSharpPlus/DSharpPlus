namespace DSharpPlus.Entities.Interaction.Components;

public sealed class DiscordUnfurledMediaItem
{
    /// <summary>
    /// Gets the URL of the media item.
    /// </summary>
    public string Url { get; internal set; }

    public DiscordUnfurledMediaItem(string url)
    {
        this.Url = url;
    }
}
