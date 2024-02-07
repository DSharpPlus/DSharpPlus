using DSharpPlus.Caching;
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
    public CachedEntity<ulong, DiscordMessage> Message { get; internal set; }

    /// <summary>
    /// Gets the channel of the original message.
    /// </summary>
    public CachedEntity<ulong, DiscordChannel> Channel { get; internal set; }

    /// <summary>
    /// Gets the guild of the original message.
    /// </summary>
    public CachedEntity<ulong, DiscordGuild> Guild { get; internal set; }

    public override string ToString()
        => $"Guild: {this.Guild.Key}, Channel: {this.Channel.Key}, Message: {this.Message.Key}";

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
