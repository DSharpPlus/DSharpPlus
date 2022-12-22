using System.Text.Json.Serialization;

namespace DSharpPlus.Entities.Internal;

public sealed record InternalBan
{
    /// <summary>
    /// The reason for the ban.
    /// </summary>
    [JsonPropertyName("reason")]
    public string? Reason { get; init; }

    /// <summary>
    /// The banned user.
    /// </summary>
    [JsonPropertyName("user")]
    public required InternalUser User { get; init; }
}
