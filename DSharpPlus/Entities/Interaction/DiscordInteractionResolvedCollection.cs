using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents Discord snowflake objects resolved from interaction arguments.
    /// </summary>
    public class DiscordInteractionResolvedCollection
    {
        /// <summary>
        /// Gets thr resolved user objects, if any.
        /// </summary>
        [JsonProperty("users", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyDictionary<ulong, DiscordUser> Users { get; internal set; }

        /// <summary>
        /// Gets thr resolved member objects, if any.
        /// </summary>
        [JsonProperty("members", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyDictionary<ulong, DiscordMember> Members { get; internal set; }

        /// <summary>
        /// Gets thr resolved channel objects, if any.
        /// </summary>
        [JsonProperty("channels", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyDictionary<ulong, DiscordChannel> Channels { get; internal set; }

        /// <summary>
        /// Gets thr resolved role objects, if any.
        /// </summary>
        [JsonProperty("roles", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyDictionary<ulong, DiscordRole> Roles { get; internal set; }

    }
}
