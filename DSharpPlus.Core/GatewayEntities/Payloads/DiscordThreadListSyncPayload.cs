using System;
using System.Collections.Generic;
using DSharpPlus.Core.Attributes;
using DSharpPlus.Core.RestEntities;
using Newtonsoft.Json;

namespace DSharpPlus.Core.GatewayEntities.Payloads
{
    /// <summary>
    /// Sent when the current user gains access to a channel.
    /// </summary>
    [DiscordGatewayPayload("THREAD_LIST_SYNC")]
    public sealed record DiscordThreadListSyncPayload
    {
        /// <summary>
        /// The id of the guild.
        /// </summary>
        [JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake GuildId { get; init; } = null!;

        /// <summary>
        /// The parent channel ids whose threads are being synced. If omitted, then threads were synced for the entire guild. This array may contain channel_ids that have no active threads as well, so you know to clear that data.
        /// </summary>
        [JsonProperty("channel_ids", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<IReadOnlyList<DiscordSnowflake>> ChannelIds { get; init; }

        /// <summary>
        /// All active threads in the given channels that the current user can access.
        /// </summary>
        [JsonProperty("threads", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<DiscordChannel> Threads { get; init; } = Array.Empty<DiscordChannel>();

        /// <summary>
        /// All thread member objects from the synced threads for the current user, indicating which threads the current user has been added to.
        /// </summary>
        [JsonProperty("members", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<DiscordThreadMember> Members { get; init; } = Array.Empty<DiscordThreadMember>();
    }
}
