using System;
using Newtonsoft.Json;

namespace DSharpPlus.Core.RestEntities
{
    /// <summary>
    /// Represents a code that when used, creates a guild based on a snapshot of an existing guild.
    /// </summary>
    public sealed record DiscordGuildTemplate
    {
        /// <summary>
        /// The template code (unique ID).
        /// </summary>
        [JsonProperty("code", NullValueHandling = NullValueHandling.Ignore)]
        public string Code { get; init; } = null!;

        /// <summary>
        /// The template name.
        /// </summary>
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; init; } = null!;

        /// <summary>
        /// The description for the template.
        /// </summary>
        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string? Description { get; init; }

        /// <summary>
        /// The number of times this template has been used.
        /// </summary>
        [JsonProperty("usage_count", NullValueHandling = NullValueHandling.Ignore)]
        public int UsageCount { get; init; }

        /// <summary>
        /// The ID of the user who created the template.
        /// </summary>
        [JsonProperty("creator_id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake CreatorId { get; init; } = null!;

        /// <summary>
        /// The user who created the template.
        /// </summary>
        [JsonProperty("creator", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordUser Creator { get; init; } = null!;

        /// <summary>
        /// When this template was created.
        /// </summary>
        [JsonProperty("created_at", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset CreatedAt { get; init; }

        /// <summary>
        /// When this template was last synced to the source guild.
        /// </summary>
        [JsonProperty("updated_at", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset UpdatedAt { get; init; }

        /// <summary>
        /// The ID of the guild this template is based on.
        /// </summary>
        [JsonProperty("source_guild_id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake SourceGuildId { get; init; } = null!;

        /// <summary>
        /// The guild snapshot this template contains.
        /// </summary>
        [JsonProperty("serialized_source_guild", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordGuild SerializedSourceGuild { get; init; } = null!;

        /// <summary>
        /// Whether the template has unsynced changes.
        /// </summary>
        [JsonProperty("is_dirty", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsDirty { get; init; }
    }
}
