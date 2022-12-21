using System;
using System.Collections.Generic;
using DSharpPlus.Core.Enums;
using DSharpPlus.Core.Entities;
using System.Text.Json.Serialization;

namespace DSharpPlus.Core.Entities.Gateway.Commands
{
    /// <summary>
    /// Sent by the client to indicate a presence or status update.
    /// </summary>
    public sealed record InternalPresenceUpdateCommand
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
        public IReadOnlyList<InternalActivity> Activities { get; init; } = Array.Empty<InternalActivity>();

        /// <summary>
        /// The user's new status.
        /// </summary>
        [JsonPropertyName("status")]
        public InternalStatusType Status { get; init; }

        /// <summary>
        /// Whether or not the client is afk.
        /// </summary>
        [JsonPropertyName("afk")]
        public bool AFK { get; init; }
    }
}
