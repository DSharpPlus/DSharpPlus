using System;

using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents a Discord rule action metadata.
/// </summary>
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
}


/// <summary>
/// Constructs auto-moderation rule action metadata.
/// </summary>
public class DiscordRuleActionMetadataBuilder
{
    /// <summary>
    /// Sets the channel which the blocked content should be logged.
    /// </summary>
    public ulong ChannelId { get; internal set; }

    /// <summary>
    /// Sets the timeout duration in seconds.
    /// </summary>
    public uint TimeoutDuration { get; internal set; }

    /// Sets the timeout duration in seconds.
    /// <summary>
    /// Gets the message that will be shown on the user screen whenever the message is blocked.
    /// </summary>
    public string? CustomMessage { get; internal set; }

    /// <summary>
    /// Add the channel id in which the blocked content will be logged.
    /// </summary>
    /// <param name="channelId">The channel id.</param>
    /// <returns></returns>
    public DiscordRuleActionMetadataBuilder WithLogChannelId(ulong channelId)
    {
        this.ChannelId = channelId;

        return this;
    }

    /// <summary>
    /// Add the timeout duration in seconds that will be applied on the member which triggered the rule.
    /// </summary>
    /// <param name="timeoutDurationInSeconds">Timeout duration.</param>
    /// <returns>This builder.</returns>
    public DiscordRuleActionMetadataBuilder WithTimeoutDuration(uint timeoutDurationInSeconds)
    {
        this.TimeoutDuration = timeoutDurationInSeconds;

        return this;
    }

    /// <summary>
    /// Add the custom message which will be shown when the rule will be triggered.
    /// </summary>
    /// <param name="message">Message to show.</param>
    /// <returns>This builder.</returns>
    /// <exception cref="ArgumentException"></exception>
    public DiscordRuleActionMetadataBuilder WithCustomMessage(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            throw new ArgumentException("Message can't be null or empty.");
        }

        this.CustomMessage = message;

        return this;
    }

    /// <summary>
    /// Build the rule action.
    /// </summary>
    /// <returns>The builded rule action.</returns>
    public DiscordRuleActionMetadata Build() => new DiscordRuleActionMetadata
    {
        ChannelId = this.ChannelId,
        TimeoutDuration = this.TimeoutDuration,
        CustomMessage = this.CustomMessage,
    };
}
