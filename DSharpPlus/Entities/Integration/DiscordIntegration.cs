namespace DSharpPlus.Entities;

using System;
using Newtonsoft.Json;

/// <summary>
/// Represents a Discord integration. These appear on the profile as linked 3rd party accounts.
/// </summary>
public class DiscordIntegration : SnowflakeObject
{
    /// <summary>
    /// Gets the integration name.
    /// </summary>
    [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
    public string Name { get; internal set; }

    /// <summary>
    /// Gets the integration type.
    /// </summary>
    [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
    public string Type { get; internal set; }

    /// <summary>
    /// Gets whether this integration is enabled.
    /// </summary>
    [JsonProperty("enabled", NullValueHandling = NullValueHandling.Ignore)]
    public bool IsEnabled { get; internal set; }

    /// <summary>
    /// Gets whether this integration is syncing.
    /// </summary>
    [JsonProperty("syncing", NullValueHandling = NullValueHandling.Ignore)]
    public bool IsSyncing { get; internal set; }

    /// <summary>
    /// Gets ID of the role this integration uses for subscribers.
    /// </summary>
    [JsonProperty("role_id", NullValueHandling = NullValueHandling.Ignore)]
    public ulong RoleId { get; internal set; }

    /// <summary>
    /// Gets the expiration behaviour.
    /// </summary>
    [JsonProperty("expire_behavior", NullValueHandling = NullValueHandling.Ignore)]
    public int ExpireBehavior { get; internal set; }

    /// <summary>
    /// Gets the grace period before expiring subscribers.
    /// </summary>
    [JsonProperty("expire_grace_period", NullValueHandling = NullValueHandling.Ignore)]
    public int ExpireGracePeriod { get; internal set; }

    /// <summary>
    /// Gets the user that owns this integration.
    /// </summary>
    [JsonProperty("user", NullValueHandling = NullValueHandling.Ignore)]
    public DiscordUser User { get; internal set; }

    /// <summary>
    /// Gets the 3rd party service account for this integration.
    /// </summary>
    [JsonProperty("account", NullValueHandling = NullValueHandling.Ignore)]
    public DiscordIntegrationAccount Account { get; internal set; }

    /// <summary>
    /// Gets the date and time this integration was last synced.
    /// </summary>
    [JsonProperty("synced_at", NullValueHandling = NullValueHandling.Ignore)]
    public DateTimeOffset SyncedAt { get; internal set; }

    internal DiscordIntegration() { }
}
