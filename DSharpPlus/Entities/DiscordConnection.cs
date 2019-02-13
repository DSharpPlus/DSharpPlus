﻿using System.Collections.Generic;
using Newtonsoft.Json;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Gets a Discord connection to a 3rd party service.
    /// </summary>
    public class DiscordConnection
    {
        /// <summary>
        /// Gets the id of the connection account
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; internal set; }

        /// <summary>
        /// Gets the username of the connection account.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets the service of the connection (twitch, youtube, steam, twitter, facebook, spotify, leagueoflegends, reddit)
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets whether the connection is revoked.
        /// </summary>
        [JsonProperty("revoked")]
        public bool IsRevoked { get; internal set; }

        /// <summary>
        /// Gets a collection of partial server integrations.
        /// </summary>
        [JsonProperty("integrations", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<DiscordIntegration> Integrations { get; internal set; }

        /// <summary>
        /// Gets the connection is verified or not.
        /// </summary>
        [JsonProperty("verified", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Verified { get; set; }

        /// <summary>
        /// Gets the connection will show activity or not.
        /// </summary>
        [JsonProperty("show_activity", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ShowActivity { get; set; }

        /// <summary>
        /// Gets the connection will sync friends or not.
        /// </summary>
        [JsonProperty("friend_sync", NullValueHandling = NullValueHandling.Ignore)]
        public bool? FriendSync { get; set; }

        /// <summary>
        /// Gets the visibility of the connection.
        /// </summary>
        [JsonProperty("visibility", NullValueHandling = NullValueHandling.Ignore)]
        public long? Visibility { get; set; }

        /// <summary>
        /// Gets the client instance this object is tied to.
        /// </summary>
        [JsonIgnore]
        internal BaseDiscordClient Discord { get; set; }

        internal DiscordConnection() { }
    }
}
