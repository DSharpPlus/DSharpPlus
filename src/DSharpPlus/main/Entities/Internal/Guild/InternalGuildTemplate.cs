using System;
using System.Text.Json.Serialization;

namespace DSharpPlus.Core.Entities
{
    /// <summary>
    /// Represents a code that when used, creates a guild based on a snapshot of an existing guild.
    /// </summary>
    public sealed record InternalGuildTemplate
    {
        /// <summary>
        /// The template code (unique ID).
        /// </summary>
        [JsonPropertyName("code")]
        public string Code { get; init; } = null!;

        /// <summary>
        /// The template name.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; init; } = null!;

        /// <summary>
        /// The description for the template.
        /// </summary>
        [JsonPropertyName("description")]
        public string? Description { get; init; }

        /// <summary>
        /// The number of times this template has been used.
        /// </summary>
        [JsonPropertyName("usage_count")]
        public int UsageCount { get; init; }

        /// <summary>
        /// The ID of the user who created the template.
        /// </summary>
        [JsonPropertyName("creator_id")]
        public InternalSnowflake CreatorId { get; init; } = null!;

        /// <summary>
        /// The user who created the template.
        /// </summary>
        [JsonPropertyName("creator")]
        public InternalUser Creator { get; init; } = null!;

        /// <summary>
        /// When this template was created.
        /// </summary>
        [JsonPropertyName("created_at")]
        public DateTimeOffset CreatedAt { get; init; }

        /// <summary>
        /// When this template was last synced to the source guild.
        /// </summary>
        [JsonPropertyName("updated_at")]
        public DateTimeOffset UpdatedAt { get; init; }

        /// <summary>
        /// The ID of the guild this template is based on.
        /// </summary>
        [JsonPropertyName("source_guild_id")]
        public InternalSnowflake SourceGuildId { get; init; } = null!;

        /// <summary>
        /// The guild snapshot this template contains.
        /// </summary>
        [JsonPropertyName("serialized_source_guild")]
        public InternalGuild SerializedSourceGuild { get; init; } = null!;

        /// <summary>
        /// Whether the template has unsynced changes.
        /// </summary>
        [JsonPropertyName("is_dirty")]
        public bool? IsDirty { get; init; }
    }
}
