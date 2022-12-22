using System.Text.Json.Serialization;

namespace DSharpPlus.Entities.Internal;

public sealed record InternalActivity
{
    /// <summary>
    /// The activity's name.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    /// <summary>
    ///The activity type.
    /// </summary>
    [JsonPropertyName("type")]
    public required DiscordActivityType Type { get; init; }

    /// <summary>
    /// The stream url, is validated when type is <see cref="DiscordActivityType.Streaming"/>.
    /// </summary>
    [JsonPropertyName("url")]
    public Optional<string?> Url { get; init; }

    /// <summary>
    /// A unix timestamp (in milliseconds) of when the activity was added to the user's session.
    /// </summary>
    [JsonPropertyName("created_at")]
    public required int CreatedAt { get; init; }

    /// <summary>
    /// Timestamps for start and/or end of the game.
    /// </summary>
    [JsonPropertyName("timestamps")]
    public Optional<InternalActivityTimestamps> Timestamps { get; init; }

    /// <summary>
    /// The application id for the game.
    /// </summary>
    [JsonPropertyName("application_id")]
    public Optional<Snowflake> ApplicationId { get; init; }

    /// <summary>
    /// What the player is currently doing.
    /// </summary>
    [JsonPropertyName("details")]
    public Optional<string?> Details { get; init; }

    /// <summary>
    /// The user's current party status.
    /// </summary>
    [JsonPropertyName("state")]
    public Optional<string?> State { get; init; }

    /// <summary>
    /// The emoji used for a custom status.
    /// </summary>
    [JsonPropertyName("emoji")]
    public Optional<InternalActivityEmoji?> Emoji { get; init; }

    /// <summary>
    /// The information for the current party of the player.
    /// </summary>
    [JsonPropertyName("party")]
    public Optional<InternalActivityParty> Party { get; init; }

    /// <summary>
    /// Images for the presence and their hover texts.
    /// </summary>
    [JsonPropertyName("assets")]
    public Optional<InternalActivityAssets> Assets { get; init; }

    /// <summary>
    /// Secrets for Rich Presence joining and spectating.
    /// </summary>
    [JsonPropertyName("secrets")]
    public Optional<InternalActivitySecrets> Secrets { get; init; }

    /// <summary>
    /// Whether or not the activity is an instanced game session.
    /// </summary>
    [JsonPropertyName("instance")]
    public Optional<bool> Instance { get; init; }

    /// <summary>
    /// <see cref="DiscordActivityFlags"/> <c>OR</c>'d together, describes what the payload includes.
    /// </summary>
    [JsonPropertyName("flags")]
    public Optional<DiscordActivityFlags> Flags { get; init; }

    /// <summary>
    /// The custom buttons shown in the Rich Presence (max 2).
    /// </summary>
    [JsonPropertyName("buttons")]
    public Optional<InternalActivityButton> Buttons { get; init; }
}
