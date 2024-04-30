namespace DSharpPlus.Entities;

using System;
using Newtonsoft.Json;

public class DiscordGuildTemplate
{
    /// <summary>
    /// Gets the template code.
    /// </summary>
    [JsonProperty("code", NullValueHandling = NullValueHandling.Ignore)]
    public string Code { get; internal set; }

    /// <summary>
    /// Gets the name of the template.
    /// </summary>
    [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
    public string Name { get; internal set; }

    /// <summary>
    /// Gets the description of the template.
    /// </summary>
    [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
    public string Description { get; internal set; }

    /// <summary>
    /// Gets the number of times the template has been used.
    /// </summary>
    [JsonProperty("usage_count", NullValueHandling = NullValueHandling.Ignore)]
    public int UsageCount { get; internal set; }

    /// <summary>
    /// Gets the ID of the creator of the template.
    /// </summary>
    [JsonProperty("creator_id", NullValueHandling = NullValueHandling.Ignore)]
    public ulong CreatorId { get; internal set; }

    /// <summary>
    /// Gets the creator of the template.
    /// </summary>
    [JsonProperty("creator", NullValueHandling = NullValueHandling.Ignore)]
    public DiscordUser Creator { get; internal set; }

    /// <summary>
    /// Date the template was created.
    /// </summary>
    [JsonProperty("created_at", NullValueHandling = NullValueHandling.Ignore)]
    public DateTimeOffset CreatedAt { get; internal set; }

    /// <summary>
    /// Date the template was updated.
    /// </summary>
    [JsonProperty("updated_at", NullValueHandling = NullValueHandling.Ignore)]
    public DateTimeOffset UpdatedAt { get; internal set; }

    /// <summary>
    /// Gets the ID of the source guild.
    /// </summary>
    [JsonProperty("source_guild_id", NullValueHandling = NullValueHandling.Ignore)]
    public ulong SourceGuildId { get; internal set; }

    /// <summary>
    /// Gets the source guild.
    /// </summary>
    [JsonProperty("serialized_source_guild", NullValueHandling = NullValueHandling.Ignore)]
    public DiscordGuild SourceGuild { get; internal set; }

    /// <summary>
    /// Gets whether the template has not synced changes.
    /// </summary>
    [JsonProperty("is_dirty", NullValueHandling = NullValueHandling.Ignore)]
    public bool? IsDirty { get; internal set; }
}
