using DSharpPlus.Core.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

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
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake Id { get; init; } = null!;

        /// <summary>
        /// Either 0 (role) or 1 (member).
        /// </summary>
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public int Type { get; init; }

        /// <summary>
        /// Permission bit set.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("allow", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordPermissions Allow { get; init; }

        /// <summary>
        /// Permission bit set.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("deny", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordPermissions Deny { get; init; }

        public static implicit operator ulong(DiscordChannelOverwrite channelOverwrite) => channelOverwrite.Id;
        public static implicit operator DiscordSnowflake(DiscordChannelOverwrite channelOverwrite) => channelOverwrite.Id;
    }
}
