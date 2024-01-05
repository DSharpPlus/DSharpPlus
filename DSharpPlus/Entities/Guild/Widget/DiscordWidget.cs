using System.Collections.Generic;
using DSharpPlus.Caching;
using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents a Discord guild's widget.
/// </summary>
public class DiscordWidget : SnowflakeObject
{
    [JsonIgnore]
    public CachedEntity<ulong, DiscordGuild> Guild { get; internal set; }

    /// <summary>
    /// Gets the guild's name.
    /// </summary>
    [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
    public string Name { get; internal set; }

    /// <summary>
    /// Gets the guild's invite URL.
    /// </summary>
    [JsonProperty("instant_invite", NullValueHandling = NullValueHandling.Ignore)]
    public string InstantInviteUrl { get; internal set; }

    /// <summary>
    /// Gets the number of online members.
    /// </summary>
    [JsonProperty("presence_count", NullValueHandling = NullValueHandling.Ignore)]
    public int PresenceCount { get; internal set; }

    /// <summary>
    /// Gets a list of online members.
    /// </summary>
    [JsonProperty("members", NullValueHandling = NullValueHandling.Ignore)]
    public IReadOnlyList<DiscordWidgetMember> Members { get; internal set; }

    /// <summary>
    /// Gets a list of widget channels.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyList<DiscordChannel> Channels { get; internal set; }
}
