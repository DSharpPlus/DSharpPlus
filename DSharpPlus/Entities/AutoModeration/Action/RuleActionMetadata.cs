using System;

using Newtonsoft.Json;

namespace DSharpPlus.Entities;

public class RuleActionMetadata
{
    /// <summary>
    /// Gets the channel which the blocked content should be logged.
    /// </summary>
    [JsonProperty("channel_id")]
    public ulong ChannelId { get; internal set; }

    /// <summary>
    /// </summary>
    [JsonProperty("duration_seconds")]
    public uint TimeoutDuration { get; internal set; }

    /// Gets the timeout duration in seconds.
    /// <summary>
    /// Gets the message that will be shown on the user screen whenever the message is blocked.
    /// </summary>
    [JsonProperty("custom_message", NullValueHandling = NullValueHandling.Ignore)]
    public string? CustomMessage { get; internal set; }

    public RuleActionMetadata()
    {

    }

    internal RuleActionMetadata(ulong channelId, uint timeoutDuration, string? customMessage)
    {
        this.ChannelId = channelId;
        this.TimeoutDuration = timeoutDuration;
        this.CustomMessage = customMessage;
    }
}

public class RuleActionMetadataBuilder
{
    /// <summary>
    /// Sets the channel which the blocked content should be logged.
    /// </summary>
    public ulong ChannelId { internal get; set; }

    /// <summary>
    /// Sets the timeout duration in seconds.
    /// </summary>
    /// <remarks>
    /// Maximum value is 2419200 (4 weeks).
    /// </remarks>
    public uint TimeoutDuration { internal get; set; }

    /// <summary>
    /// Sets the message that will be shown on the user screen whenever the message is blocked.
    /// </summary>
    public string? CustomMessage { internal get; set; }

    public RuleActionMetadataBuilder()
    {

    }

    public RuleActionMetadata Build()
    {
        if (TimeoutDuration > 2419200)
        {
            throw new ArgumentException("Value can't be bigger than 2419200.");
        }
        else if (CustomMessage is not null && CustomMessage.Length > 150) 
        {
            throw new ArgumentException("Custom message length can't be bigger than 150 characters.");
        }

        return new RuleActionMetadata(this.ChannelId, this.TimeoutDuration, this.CustomMessage);
    }
}
