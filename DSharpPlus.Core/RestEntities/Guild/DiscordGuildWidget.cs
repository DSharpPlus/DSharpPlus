using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DSharpPlus.Core.RestEntities
{
    public sealed record DiscordGuildWidget
    {
        /// <summary>
        /// The guild id.
        /// </summary>
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake Id { get; init; } = null!;

        /// <summary>
        /// The guild name (2-100 characters).
        /// </summary>
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; init; } = null!;

        /// <summary>
        /// The instant invite for the guilds specified widget invite channel.
        /// </summary>
        [JsonProperty("instant_invite", NullValueHandling = NullValueHandling.Ignore)]
        public string? InstantInvite { get; init; }

        /// <summary>
        /// The voice and stage channels which are accessible by @everyone
        /// </summary>
        [JsonProperty("channels", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<DiscordChannel> Channels { get; init; } = Array.Empty<DiscordChannel>();

        /// <summary>
        /// A list of special widget user objects that includes users presence (Limit 100).
        /// </summary>
        /// <remarks>
        /// The fields <see cref="DiscordUser.Id"/>, <see cref="DiscordUser.Discriminator"/> and <see cref="DiscordUser.Avatar"/>, are anonymized to prevent abuse.
        /// <b>This field is full of <see cref="DiscordUser"/> objects, NOT <see cref="DiscordGuildMember"/> objects.</b>
        /// </remarks>
        [JsonProperty("members", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<DiscordUser> Members { get; init; } = Array.Empty<DiscordUser>();

        /// <summary>
        /// The number of online members in this guild.
        /// </summary>
        [JsonProperty("presence_count", NullValueHandling = NullValueHandling.Ignore)]
        public int PresenceCount { get; init; }
    }
}
