using System.Text.Json.Serialization;

namespace DSharpPlus.Entities.Internal;

/// <summary>
/// Used to represent a webhook.
/// </summary>
public sealed record InternalWebhook
{
    /// <summary>
    /// The id of the webhook.
    /// </summary>
    [JsonPropertyName("id")]
    public required Snowflake Id { get; init; } 

    /// <summary>
    /// The <see cref="DiscordWebhookType"/> of the webhook.
    /// </summary>
    [JsonPropertyName("type")]
    public required DiscordWebhookType Type { get; init; }

    /// <summary>
    /// The guild id this webhook is for, if any.
    /// </summary>
    [JsonPropertyName("guild_id")]
    public Optional<Snowflake?> GuildId { get; init; }

    /// <summary>
    /// The channel id this webhook is for, if any.
    /// </summary>
    [JsonPropertyName("channel_id")]
    public Snowflake? ChannelId { get; init; }

    /// <summary>
    /// The user this webhook was created by (not returned when getting a webhook with its token).
    /// </summary>
    [JsonPropertyName("user")]
    public Optional<InternalUser> User { get; init; }

    /// <summary>
    /// The default name of the webhook.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    /// <summary>
    /// The default user avatar hash of the webhook.
    /// </summary>
    [JsonPropertyName("avatar")]
    public string? Avatar { get; init; }

    /// <summary>
    /// The secure token of the webhook (returned for Incoming Webhooks).
    /// </summary>
    [JsonPropertyName("token")]
    public Optional<string> Token { get; init; }

    /// <summary>
    /// The bot/OAuth2 application that created this webhook.
    /// </summary>
    [JsonPropertyName("application_id")]
    public Snowflake? ApplicationId { get; init; }

    /// <summary>
    /// The guild of the channel that this webhook is following (returned for Channel Follower Webhooks).
    /// </summary>
    [JsonPropertyName("source_guild")]
    public Optional<InternalGuild> SourceGuild { get; init; }

    /// <summary>
    /// The channel that this webhook is following (returned for Channel Follower Webhooks).
    /// </summary>
    [JsonPropertyName("source_channel")]
    public Optional<InternalChannel> SourceChannel { get; init; }

    /// <summary>
    /// The url used for executing the webhook (returned by the webhooks OAuth2 flow).
    /// </summary>
    [JsonPropertyName("url")]
    public Optional<string> Url { get; init; }
}
