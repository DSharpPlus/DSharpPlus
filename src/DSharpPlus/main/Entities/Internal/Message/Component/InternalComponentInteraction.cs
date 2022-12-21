using System.Text.Json.Serialization;

namespace DSharpPlus.Entities.Internal;

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
    public Snowflake Id { get; init; } = null!;

    [JsonPropertyName("guild_id")]
    public Snowflake GuildId { get; init; } = null!;

    [JsonPropertyName("data")]
    public InternalInteractionResolvedData Data { get; init; } = null!;

    [JsonPropertyName("channel_id")]
    public Snowflake ChannelId { get; init; } = null!;

    [JsonPropertyName("application_id")]
    public Snowflake ApplicationId { get; init; } = null!;
}
