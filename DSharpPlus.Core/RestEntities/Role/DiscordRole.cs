using DSharpPlus.Core.Enums;
using Newtonsoft.Json;

namespace DSharpPlus.Core.RestEntities
{
    /// <summary>
    /// Roles represent a set of permissions attached to a group of users. Roles have names, colors, and can be "pinned" to the side bar, causing their members to be listed separately. Roles can have separate permission profiles for the global context (guild) and channel context. The <c>@everyone</c> role has the same ID as the guild it belongs to.
    /// </summary>
    public sealed record DiscordRole
    {
        /// <summary>
        /// Role Id.
        /// </summary>
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake Id { get; init; } = null!;

        /// <summary>
        /// Name of the role.
        /// </summary>
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; init; } = null!;

        /// <summary>
        /// The color of the role.
        /// </summary>
        [JsonProperty("color", NullValueHandling = NullValueHandling.Ignore)]
        public int Color { get; init; }

        /// <summary>
        /// If this role is pinned in the user listing.
        /// </summary>
        [JsonProperty("hoist", NullValueHandling = NullValueHandling.Ignore)]
        public bool Hoist { get; init; }

        /// <summary>
        /// The role icon hash.
        /// </summary>
        [JsonProperty("icon", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<string?> Icon { get; init; }

        /// <summary>
        /// The role unicode emoji.
        /// </summary>
        [JsonProperty("unicode_emoji", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<string?> UnicodeEmoji { get; init; }

        /// <summary>
        /// The position of this role.
        /// </summary>
        [JsonProperty("position", NullValueHandling = NullValueHandling.Ignore)]
        public int Position { get; init; }

        /// <summary>
        /// The Discord permissions of this role.
        /// </summary>
        [JsonProperty("permissions", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordPermissions Permissions { get; init; }

        /// <summary>
        /// Whether this role is managed by an integration.
        /// </summary>
        [JsonProperty("managed", NullValueHandling = NullValueHandling.Ignore)]
        public bool Managed { get; init; }

        /// <summary>
        /// Whether this role is mentionable.
        /// </summary>
        [JsonProperty("mentionable", NullValueHandling = NullValueHandling.Ignore)]
        public bool Mentionable { get; init; }

        /// <summary>
        /// The tags this role has.
        /// </summary>
        [JsonProperty("tags", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordRoleTags> Tags { get; init; }

        public static implicit operator ulong(DiscordRole role) => role.Id;
        public static implicit operator DiscordSnowflake(DiscordRole role) => role.Id;
    }
}
