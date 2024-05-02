using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents the channel to which an invite is linked.
/// </summary>
public class DiscordInviteChannel : SnowflakeObject
{
    /// <summary>
    /// Gets the name of the channel.
    /// </summary>
    [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
    public string Name { get; internal set; } = default!;

    /// <summary>
    /// Gets the type of the channel.
    /// </summary>
    [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
    public DiscordChannelType Type { get; internal set; }

    internal DiscordInviteChannel() { }
}
