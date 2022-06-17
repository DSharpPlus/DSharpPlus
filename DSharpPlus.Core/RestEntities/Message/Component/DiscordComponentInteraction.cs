using System.Text.Json.Serialization;

namespace DSharpPlus.Core.RestEntities
{
    public sealed record DiscordComponentInteraction
    {
        [JsonPropertyName("version")]
        public int Version { get; init; }

        [JsonPropertyName("type")]
        public int Type { get; init; }

        [JsonPropertyName("message")]
        public DiscordMessage Message { get; init; } = null!;

        [JsonPropertyName("member")]
        public DiscordGuildMember Member { get; init; } = null!;

        [JsonPropertyName("id")]
        public DiscordSnowflake Id { get; init; } = null!;

        [JsonPropertyName("guild_id")]
        public DiscordSnowflake GuildId { get; init; } = null!;

        [JsonPropertyName("data")]
        public DiscordInteractionResolvedData Data { get; init; } = null!;

        [JsonPropertyName("channel_id")]
        public DiscordSnowflake ChannelId { get; init; } = null!;

        [JsonPropertyName("application_id")]
        public DiscordSnowflake ApplicationId { get; init; } = null!;
    }
}
