using System.Text.Json.Serialization;

namespace DSharpPlus.Entities.Internal;

public sealed record InternalIntegrationApplication
{
    /// <summary>
    /// The id of the app.
    /// </summary>
    [JsonPropertyName("id")]
    public InternalSnowflake Id { get; init; } = null!;

    /// <summary>
    /// The name of the app.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; init; } = null!;

    /// <summary>
    /// The icon hash of the app.
    /// </summary>
    [JsonPropertyName("icon")]
    public string? Icon { get; init; }

    /// <summary>
    /// The description of the app.
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; init; } = null!;

    /// <summary>
    /// The bot associated with this application.
    /// </summary>
    [JsonPropertyName("bot")]
    public Optional<InternalUser> Bot { get; init; }
}
