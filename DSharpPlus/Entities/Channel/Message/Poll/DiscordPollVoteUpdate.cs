using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents an update for a poll vote.
/// </summary>
public class DiscordPollVoteUpdate
{
    /// <summary>
    /// Gets or sets a client for this vote update.
    /// </summary>
    internal DiscordClient _client;

    /// <summary>
    /// Gets whether this vote was added or removed. <c>true</c> if it was added, <c>false</c> if it was removed.
    /// </summary>
    [JsonIgnore]
    public bool WasAdded { get; internal set; }

    /// <summary>
    /// Gets the user that added or removed a vote.
    /// </summary>
    public DiscordUser User => _client.GetCachedOrEmptyUserInternal(UserId);

    [JsonIgnore]
    public DiscordChannel Channel => _client.InternalGetCachedChannel(ChannelId);

    /// <summary>
    /// Gets the message that the poll is attached to.
    /// </summary>
    /// <remarks>
    /// This property attempts to pull the associated message from cache, which relies on a cache provider
    /// being enabled in the client. If no cache provider is enabled, this property will always return <see langword="null"/>.
    /// </remarks>
    // Should this pull from cache as an auto-property? Perhaps having a hard-set message pulled from cache further up
    // instead. 
    [JsonIgnore]
    public DiscordMessage? Message
        => _client.MessageCache?.TryGet(MessageId, out DiscordMessage? msg) ?? false ? msg : null;

    /// <summary>
    /// Gets the guild this poll was sent in, if applicable.
    /// </summary>
    public DiscordGuild? Guild
        => GuildId.HasValue ? _client.InternalGetCachedGuild(GuildId.Value) : null;

    [JsonProperty("user_id")]
    internal ulong UserId { get; set; }

    [JsonProperty("channel_id")]
    internal ulong ChannelId { get; set; }

    [JsonProperty("message_id")]
    internal ulong MessageId { get; set; }

    [JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
    internal ulong? GuildId { get; set; }

    [JsonProperty("answer_id")]
    internal int AnswerId { get; set; }

    internal DiscordPollVoteUpdate() { }
}
