using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DSharpPlus.Core.Entities
{
    public sealed record InternalGuildPreview
    {
        /// <summary>
        /// The guild id.
        /// </summary>
        [JsonPropertyName("id")]
        public InternalSnowflake Id { get; init; } = null!;

        /// <summary>
        /// The guild name (2-100 characters).
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; init; } = null!;

        /// <summary>
        /// The <see href="https://discord.com/developers/docs/reference#image-formatting">icon hash</see>.
        /// </summary>
        [JsonPropertyName("icon")]
        public string? Icon { get; init; }

        /// <summary>
        /// The <see href="https://discord.com/developers/docs/reference#image-formatting">splash hash</see>.
        /// </summary>
        [JsonPropertyName("splash")]
        public string? Splash { get; init; }

        /// <summary>
        /// The <see href="https://discord.com/developers/docs/reference#image-formatting">discovery splash hash</see>.
        /// </summary>
        [JsonPropertyName("discovery_splash")]
        public string? DiscoverySplash { get; init; }

        /// <summary>
        /// The guild's custom emojis.
        /// </summary>
        [JsonPropertyName("emojis")]
        public IReadOnlyList<InternalEmoji> Emojis { get; init; } = Array.Empty<InternalEmoji>();

        /// <summary>
        /// The enabled guild features.
        /// </summary>
        /// <remarks>
        /// See <see cref="InternalGuildFeature"/> for more information.
        /// </remarks>
        [JsonPropertyName("features")]
        public string[] Features { get; init; } = Array.Empty<string>();

        /// <summary>
        /// The approximate number of members in this guild.
        /// </summary>
        [JsonPropertyName("approximate_member_count")]
        public int ApproximateMemberCount { get; init; }

        /// <summary>
        /// The approximate number of online members in this guild.
        /// </summary>
        [JsonPropertyName("approximate_presence_count")]
        public int ApproximatePresenceCount { get; init; }

        /// <summary>
        /// The description for the guild.
        /// </summary>
        [JsonPropertyName("description")]
        public string? Description { get; init; }

        /// <summary>
        /// The guild's custom stickers.
        /// </summary>
        [JsonPropertyName("stickers")]
        public IReadOnlyList<InternalSticker> Stickers { get; init; } = Array.Empty<InternalSticker>();
    }
}
