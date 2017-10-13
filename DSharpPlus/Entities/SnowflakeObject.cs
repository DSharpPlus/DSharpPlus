﻿using System;
using Newtonsoft.Json;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents an object in Discord API.
    /// </summary>
    public abstract class SnowflakeObject
    {
        /// <summary>
        /// Gets the ID of this object.
        /// </summary>
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong Id { get; set; }

        /// <summary>
        /// Gets the date and time this object was created.
        /// </summary>
        [JsonIgnore]
        public DateTimeOffset CreationTimestamp => new DateTimeOffset(2015, 1, 1, 0, 0, 0, TimeSpan.Zero).AddMilliseconds(Id >> 22);

        /// <summary>
        /// Gets the client instance this object is tied to.
        /// </summary>
        [JsonIgnore]
        internal BaseDiscordClient Discord { get; set; }

        internal SnowflakeObject() { }
    }
}
