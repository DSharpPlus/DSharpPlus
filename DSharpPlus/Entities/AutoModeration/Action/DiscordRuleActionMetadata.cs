using Newtonsoft.Json;

namespace DSharpPlus.Entities;

public class DiscordRuleActionMetadata
{
    /// <summary>
    /// Gets the channel which the blocked content should be logged.
    /// </summary>
    [JsonProperty("channel_id")]
    public ulong ChannelId { get; internal set; }

    /// <summary>
    /// Gets the timeout duration in seconds.
    /// </summary>
    [JsonProperty("duration_seconds")]
    public uint TimeoutDuration { get; internal set; }

    /// Gets the timeout duration in seconds.
    /// <summary>
    /// Gets the message that will be shown on the user screen whenever the message is blocked.
    /// </summary>
    [JsonProperty("custom_message", NullValueHandling = NullValueHandling.Ignore)]
    public string? CustomMessage { get; internal set; }

    public DiscordRuleActionMetadata()
    {

    }
}
