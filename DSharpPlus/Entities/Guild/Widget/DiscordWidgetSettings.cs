using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents a Discord guild's widget settings.
/// </summary>
public class DiscordWidgetSettings
{
    internal DiscordGuild Guild { get; set; }

    /// <summary>
    /// Gets the guild's widget channel id.
    /// </summary>
    [JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
    public ulong ChannelId { get; internal set; }

    /// <summary>
    /// Gets the guild's widget channel.
    /// </summary>
    public DiscordChannel Channel
        => this.Guild?.GetChannel(this.ChannelId);

    /// <summary>
    /// Gets if the guild's widget is enabled.
    /// </summary>
    [JsonProperty("enabled", NullValueHandling = NullValueHandling.Ignore)]
    public bool IsEnabled { get; internal set; }
}
