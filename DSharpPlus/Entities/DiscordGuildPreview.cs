using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using DSharpPlus.Net.Serialization;
using Newtonsoft.Json;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents a Discord integration. These appear on the profile as linked 3rd party accounts.
    /// </summary>
    public class DiscordGuildPreview : SnowflakeObject
    {
        /// <summary>
        /// Gets the integration name.
        /// </summary>
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; internal set; }

        /// <summary>
        /// Gets the integration type.
        /// </summary>
        [JsonProperty("icon", NullValueHandling = NullValueHandling.Ignore)]
        public string Icon { get; internal set; }


        /// <summary>
        /// Gets the integration type.
        /// </summary>
        [JsonProperty("splash", NullValueHandling = NullValueHandling.Ignore)]
        public string Splash { get; internal set; }


        /// <summary>
        /// Gets the integration type.
        /// </summary>
        [JsonProperty("discovery_splash", NullValueHandling = NullValueHandling.Ignore)]
        public string DiscoverySplash { get; internal set; }



        /// <summary>
        /// Gets a collection of this guild's emojis.
        /// </summary>
        [JsonIgnore]
        public IReadOnlyDictionary<ulong, DiscordEmoji> Emojis => new ReadOnlyConcurrentDictionary<ulong, DiscordEmoji>(this._emojis);

        [JsonProperty("emojis", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(SnowflakeArrayAsDictionaryJsonConverter))]
        internal ConcurrentDictionary<ulong, DiscordEmoji> _emojis;


        /// <summary>
        /// Gets a collection of this guild's features.
        /// </summary>
        [JsonProperty("features", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<string> Features { get; internal set; }

        /// <summary>
        /// Gets the integration type.
        /// </summary>
        [JsonProperty("approximate_member_count")]
        public int ApproximateMemberCount { get; internal set; }

        /// <summary>
        /// Gets the integration type.
        /// </summary>
        [JsonProperty("approximate_presence_count")]
        public int ApproximatePresenceCount { get; internal set; }


        /// <summary>
        /// Gets the integration type.
        /// </summary>
        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; internal set; }

        internal DiscordGuildPreview() { }
    }
}
