using System.Collections.Concurrent;
using System.Collections.Generic;
using DSharpPlus.Net.Serialization;
using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents a guild preview.
/// </summary>
public class DiscordGuildPreview : SnowflakeObject
{
    /// <summary>
    /// Gets the guild's name.
    /// </summary>
    [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
    public string Name { get; internal set; }

    /// <summary>
    /// Gets the guild's icon.
    /// </summary>
    [JsonProperty("icon", NullValueHandling = NullValueHandling.Ignore)]
    public string Icon { get; internal set; }

    /// <summary>
    /// Gets the guild's splash.
    /// </summary>
    [JsonProperty("splash", NullValueHandling = NullValueHandling.Ignore)]
    public string Splash { get; internal set; }

    /// <summary>
    /// Gets the guild's discovery splash.
    /// </summary>
    [JsonProperty("discovery_splash", NullValueHandling = NullValueHandling.Ignore)]
    public string DiscoverySplash { get; internal set; }

    /// <summary>
    /// Gets a collection of this guild's emojis.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyDictionary<ulong, DiscordEmoji> Emojis => new ReadOnlyConcurrentDictionary<ulong, DiscordEmoji>(this.emojis);

    [JsonProperty("emojis", NullValueHandling = NullValueHandling.Ignore)]
    [JsonConverter(typeof(SnowflakeArrayAsDictionaryJsonConverter))]
    internal ConcurrentDictionary<ulong, DiscordEmoji> emojis;

    /// <summary>
    /// Gets a collection of this guild's features.
    /// </summary>
    [JsonProperty("features", NullValueHandling = NullValueHandling.Ignore)]
    public IReadOnlyList<string> Features { get; internal set; }

    /// <summary>
    /// Gets the approximate member count.
    /// </summary>
    [JsonProperty("approximate_member_count")]
    public int ApproximateMemberCount { get; internal set; }

    /// <summary>
    /// Gets the approximate presence count.
    /// </summary>
    [JsonProperty("approximate_presence_count")]
    public int ApproximatePresenceCount { get; internal set; }

    /// <summary>
    /// Gets the description for the guild, if the guild is discoverable.
    /// </summary>
    [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
    public string Description { get; internal set; }

    internal DiscordGuildPreview() { }
}
