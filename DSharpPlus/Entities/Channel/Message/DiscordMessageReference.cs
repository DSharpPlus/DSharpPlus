using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents data from the original message.
/// </summary>
public class DiscordMessageReference
{
    /// <summary>
    /// Gets the original message.
    /// </summary>
    public DiscordMessage Message { get; internal set; } = default!;

    /// <summary>
    /// Gets the channel of the original message.
    /// </summary>
    public DiscordChannel Channel { get; internal set; } = default!;

    /// <summary>
    /// Gets the guild of the original message.
    /// </summary>
    public DiscordGuild? Guild { get; internal set; }

    public override string ToString()
        => $"Guild: {this.Guild?.Id ?? 0}, Channel: {this.Channel.Id}, Message: {this.Message.Id}";

    internal DiscordMessageReference() { }
}

internal struct InternalDiscordMessageReference
{
    [JsonProperty("message_id", NullValueHandling = NullValueHandling.Ignore)]
    internal ulong? MessageId { get; set; }

    [JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
    internal ulong? ChannelId { get; set; }

    [JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
    internal ulong? GuildId { get; set; }

    [JsonProperty("fail_if_not_exists", NullValueHandling = NullValueHandling.Ignore)]
    public bool FailIfNotExists { get; set; }
}
