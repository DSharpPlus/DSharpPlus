using DSharpPlus.Core.Enums;
using System.Text.Json.Serialization;

namespace DSharpPlus.Core.RestEntities
{
    /// <remarks>
    /// See <see href="https://discord.com/developers/docs/topics/permissions#permissions">permissions</see> for more information about the allow and deny fields.
    /// </remarks>
    public sealed record DiscordChannelOverwrite
    {
        /// <summary>
        /// Role or user id.
        /// </summary>
        [JsonPropertyName("id")]
        public DiscordSnowflake Id { get; init; } = null!;

        /// <summary>
        /// Either 0 (role) or 1 (member).
        /// </summary>
        [JsonPropertyName("type")]
        public int Type { get; init; }

        /// <summary>
        /// Permission bit set.
        /// </summary>
        [JsonPropertyName("allow")]
        public DiscordPermissions Allow { get; init; }

        /// <summary>
        /// Permission bit set.
        /// </summary>
        [JsonPropertyName("deny")]
        public DiscordPermissions Deny { get; init; }

        public static implicit operator ulong(DiscordChannelOverwrite channelOverwrite) => channelOverwrite.Id;
        public static implicit operator DiscordSnowflake(DiscordChannelOverwrite channelOverwrite) => channelOverwrite.Id;
    }
}
