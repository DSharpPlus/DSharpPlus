using System;
using System.Collections.Generic;
using DSharpPlus.Core.Enums;
using DSharpPlus.Core.RestEntities;
using Newtonsoft.Json;

namespace DSharpPlus.Core.GatewayEntities.Commands
{
    /// <summary>
    /// Sent by the client to indicate a presence or status update.
    /// </summary>
    public sealed record DiscordPresenceUpdateCommand
    {
        /// <summary>
        /// The unix time (in milliseconds) of when the client went idle, or null if the client is not idle.
        /// </summary>
        [JsonProperty("since", NullValueHandling = NullValueHandling.Ignore)]
        public int? Since { get; init; }

        /// <summary>
        /// The user's activities.
        /// </summary>
        [JsonProperty("activities", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<DiscordActivity> Activities { get; init; } = Array.Empty<DiscordActivity>();

        /// <summary>
        /// The user's new status.
        /// </summary>
        [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordStatusType Status { get; init; }

        /// <summary>
        /// Whether or not the client is afk.
        /// </summary>
        [JsonProperty("afk", NullValueHandling = NullValueHandling.Ignore)]
        public bool AFK { get; init; }
    }
}
