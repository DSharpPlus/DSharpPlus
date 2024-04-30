namespace DSharpPlus.Entities;
using Newtonsoft.Json;

/// <summary>
/// Metadata for a <see cref="DiscordScheduledGuildEvent"/>.
/// </summary>
public sealed class DiscordScheduledGuildEventMetadata
{
    /// <summary>
    /// If this is an external event, where this event is hosted.
    /// </summary>
    [JsonProperty("location")]
    public string Location { get; internal set; }

    internal DiscordScheduledGuildEventMetadata() { }

    public DiscordScheduledGuildEventMetadata(string location) => Location = location;
}
