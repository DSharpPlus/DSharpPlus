using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents a discord role tags.
/// </summary>
public class DiscordRoleTags
{
    /// <summary>
    /// Gets the id of the bot this role belongs to.
    /// </summary>
    [JsonProperty("bot_id", NullValueHandling = NullValueHandling.Ignore)]
    public ulong? BotId { get; internal set; }

    /// <summary>
    /// Gets the id of the integration this role belongs to.
    /// </summary>
    [JsonProperty("integration_id", NullValueHandling = NullValueHandling.Ignore)]
    public ulong? IntegrationId { get; internal set; }

    /// <summary>
    /// Gets whether this is the guild's premium subscriber role.
    /// </summary>
    [JsonIgnore]
    public bool IsPremiumSubscriber
        => this.premiumSubscriber.HasValue && this.premiumSubscriber.Value is null;

    [JsonProperty("premium_subscriber", NullValueHandling = NullValueHandling.Include)]
    internal Optional<bool?> premiumSubscriber = false;

}
