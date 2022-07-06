using System;
using System.Collections.Generic;
using DSharpPlus.Core.Enums;
using DSharpPlus.Core.RestEntities;
using System.Text.Json.Serialization;

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
        [JsonPropertyName("since")]
        public int? Since { get; init; }

        /// <summary>
        /// The user's activities.
        /// </summary>
        [JsonPropertyName("activities")]
        public IReadOnlyList<DiscordActivity> Activities { get; init; } = Array.Empty<DiscordActivity>();

        /// <summary>
        /// The user's new status.
        /// </summary>
        [JsonPropertyName("status")]
        public DiscordStatusType Status { get; init; }

        /// <summary>
        /// Whether or not the client is afk.
        /// </summary>
        [JsonPropertyName("afk")]
        public bool AFK { get; init; }
    }
}
