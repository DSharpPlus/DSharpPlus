using System.Text.Json.Serialization;

namespace DSharpPlus.Core.Entities
{
    public sealed record InternalComponentInteraction
    {
        [JsonPropertyName("version")]
        public int Version { get; init; }

        [JsonPropertyName("type")]
        public int Type { get; init; }

        [JsonPropertyName("message")]
        public InternalMessage Message { get; init; } = null!;

        [JsonPropertyName("member")]
        public InternalGuildMember Member { get; init; } = null!;

        [JsonPropertyName("id")]
        public InternalSnowflake Id { get; init; } = null!;

        [JsonPropertyName("guild_id")]
        public InternalSnowflake GuildId { get; init; } = null!;

        [JsonPropertyName("data")]
        public InternalInteractionResolvedData Data { get; init; } = null!;

        [JsonPropertyName("channel_id")]
        public InternalSnowflake ChannelId { get; init; } = null!;

        [JsonPropertyName("application_id")]
        public InternalSnowflake ApplicationId { get; init; } = null!;
    }
}
