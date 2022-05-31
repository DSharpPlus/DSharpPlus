using System;
using DSharpPlus.Core.Enums;
using Newtonsoft.Json;

namespace DSharpPlus.Core.RestEntities
{
    /// <summary>
    /// Represents a sticker that can be sent in messages.
    /// </summary>
    public sealed record DiscordSticker
    {
        /// <summary>
        /// The id of the sticker.
        /// </summary>
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake Id { get; init; } = null!;

        /// <summary>
        /// For standard stickers, id of the pack the sticker is from.
        /// </summary>
        [JsonProperty("pack_id", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordSnowflake> PackId { get; init; }

        /// <summary>
        /// The name of the sticker.
        /// </summary>
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; init; } = null!;

        /// <summary>
        /// The description of the sticker.
        /// </summary>
        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string? Description { get; init; }

        /// <summary>
        /// Autocomplete/suggestion tags for the sticker (max 200 characters).
        /// </summary>
        [JsonProperty("tags", NullValueHandling = NullValueHandling.Ignore)]
        public string Tags { get; init; } = null!;

        /// <summary>
        /// Deprecated previously the sticker asset hash, now an empty string
        /// </summary>
        [JsonProperty("asset", NullValueHandling = NullValueHandling.Ignore)]
        [Obsolete("Deprecated previously the sticker asset hash, now an empty string")]
        public Optional<string> Asset { get; set; } = null!;

        /// <summary>
        /// The type of sticker.
        /// </summary>
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordStickerType Type { get; init; }

        /// <summary>
        /// The type of sticker format.
        /// </summary>
        [JsonProperty("format_type", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordStickerFormatType FormatType { get; init; }

        /// <summary>
        /// Whether this guild sticker can be used, may be false due to loss of Server Boosts.
        /// </summary>
        [JsonProperty("available", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<bool> Available { get; init; }

        /// <summary>
        /// The id of the guild that owns this sticker.
        /// </summary>
        [JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordSnowflake?> GuildId { get; init; }

        /// <summary>
        /// The user that uploaded the guild sticker.
        /// </summary>
        [JsonProperty("user", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordUser> User { get; init; }

        /// <summary>
        /// The standard sticker's sort order within its pack.
        /// </summary>
        [JsonProperty("sort_value", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<int> SortValue { get; init; }
    }
}
