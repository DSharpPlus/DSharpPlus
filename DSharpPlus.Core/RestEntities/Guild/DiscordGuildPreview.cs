using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DSharpPlus.Core.RestEntities
{
    public sealed record DiscordGuildPreview
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
        /// The <see href="https://discord.com/developers/docs/reference#image-formatting">icon hash</see>.
        /// </summary>
        [JsonProperty("icon", NullValueHandling = NullValueHandling.Ignore)]
        public string? Icon { get; init; }

        /// <summary>
        /// The <see href="https://discord.com/developers/docs/reference#image-formatting">splash hash</see>.
        /// </summary>
        [JsonProperty("splash", NullValueHandling = NullValueHandling.Ignore)]
        public string? Splash { get; init; }

        /// <summary>
        /// The <see href="https://discord.com/developers/docs/reference#image-formatting">discovery splash hash</see>.
        /// </summary>
        [JsonProperty("discovery_splash", NullValueHandling = NullValueHandling.Ignore)]
        public string? DiscoverySplash { get; init; }

        /// <summary>
        /// The guild's custom emojis.
        /// </summary>
        [JsonProperty("emojis", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<DiscordEmoji> Emojis { get; init; } = Array.Empty<DiscordEmoji>();

        /// <summary>
        /// The enabled guild features.
        /// </summary>
        /// <remarks>
        /// See <see cref="DiscordGuildFeature"/> for more information.
        /// </remarks>
        [JsonProperty("features", NullValueHandling = NullValueHandling.Ignore)]
        public string[] Features { get; init; } = Array.Empty<string>();

        /// <summary>
        /// The approximate number of members in this guild.
        /// </summary>
        [JsonProperty("approximate_member_count", NullValueHandling = NullValueHandling.Ignore)]
        public int ApproximateMemberCount { get; init; }

        /// <summary>
        /// The approximate number of online members in this guild.
        /// </summary>
        [JsonProperty("approximate_presence_count", NullValueHandling = NullValueHandling.Ignore)]
        public int ApproximatePresenceCount { get; init; }

        /// <summary>
        /// The description for the guild.
        /// </summary>
        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string? Description { get; init; }

        /// <summary>
        /// The guild's custom stickers.
        /// </summary>
        [JsonProperty("stickers", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<DiscordSticker> Stickers { get; init; } = Array.Empty<DiscordSticker>();
    }
}
