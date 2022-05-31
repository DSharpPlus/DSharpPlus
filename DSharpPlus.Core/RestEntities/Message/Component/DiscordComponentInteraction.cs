using Newtonsoft.Json;

namespace DSharpPlus.Core.RestEntities
{
    public sealed record DiscordComponentInteraction
    {
        [JsonProperty("version", NullValueHandling = NullValueHandling.Ignore)]
        public int Version { get; init; }

        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public int Type { get; init; }

        [JsonProperty("message", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordMessage Message { get; init; } = null!;

        [JsonProperty("member", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordGuildMember Member { get; init; } = null!;

        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake Id { get; init; } = null!;

        [JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake GuildId { get; init; } = null!;

        [JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordInteractionResolvedData Data { get; init; } = null!;

        [JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake ChannelId { get; init; } = null!;

        [JsonProperty("application_id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake ApplicationId { get; init; } = null!;
    }
}
