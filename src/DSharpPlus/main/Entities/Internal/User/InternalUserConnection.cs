using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DSharpPlus.Entities.Internal;

public sealed record InternalUserConnection
{
    /// <summary>
    /// The id of the connection account.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = null!;

    /// <summary>
    /// The username of the connection account.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; init; } = null!;

    /// <summary>
    /// The service of the connection (twitch, youtube).
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; init; } = null!;

    /// <summary>
    /// Whether the connection is revoked.
    /// </summary>
    [JsonPropertyName("revoked")]
    public Optional<bool> Revoked { get; init; }

    /// <summary>
    /// An array of partial server integrations.
    /// </summary>
    [JsonPropertyName("integrations")]
    public Optional<IReadOnlyList<InternalIntegration>> Integrations { get; init; }

    /// <summary>
    /// Whether the connection is verified.
    /// </summary>
    [JsonPropertyName("verified")]
    public bool Verified { get; init; }

    /// <summary>
    /// Whether friend sync is enabled for this connection.
    /// </summary>
    [JsonPropertyName("friend_sync")]
    public bool FriendSync { get; init; }

    /// <summary>
    /// Whether activities related to this connection will be shown in presence updates.
    /// </summary>
    [JsonPropertyName("show_activity")]
    public bool ShowActivity { get; init; }

    /// <summary>
    /// The visibility of this connection.
    /// </summary>
    [JsonPropertyName("visibility")]
    public DiscordUserConnectionVisibilityType Visibility { get; init; }
}
