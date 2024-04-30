namespace DSharpPlus.Entities;
using Newtonsoft.Json;

/// <summary>
/// Represents a Discord guild widget.
/// </summary>
public class DiscordGuildEmbed
{
    /// <summary>
    /// Gets whether the embed is enabled.
    /// </summary>
    [JsonProperty("enabled", NullValueHandling = NullValueHandling.Ignore)]
    public bool IsEnabled { get; set; }

    /// <summary>
    /// Gets the ID of the widget channel.
    /// </summary>
    [JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
    public ulong ChannelId { get; set; }
}
