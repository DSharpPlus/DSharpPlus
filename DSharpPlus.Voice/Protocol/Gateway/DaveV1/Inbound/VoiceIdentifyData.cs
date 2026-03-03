using System.Text.Json.Serialization;

/// <summary>
/// Represents the data included in a voice identify payload.
/// This data is sent by the client to the Discord voice server
/// to authenticate and establish a new voice connection.
/// </summary>
public sealed record VoiceIdentifyData
{
    /// <summary>
    /// Gets or sets the ID of the server (guild) that the
    /// client is attempting to connect to.
    /// </summary>
    [JsonPropertyName("server_id")]
    public required ulong GuildId { get; init; }

    /// <summary>
    /// Gets or sets the ID of the user who is identifying
    /// with the voice server.
    /// </summary>
    [JsonPropertyName("user_id")]
    public required ulong UserId { get; init; }

    /// <summary>
    /// Gets or sets the session ID of the voice connection,
    /// provided by the Gateway during the initial setup.
    /// </summary>
    [JsonPropertyName("session_id")]
    public required string SessionId { get; init; }

    /// <summary>
    /// Gets or sets the authentication token used to validate
    /// the voice connection request.
    /// </summary>
    [JsonPropertyName("token")]
    public required string Token { get; init; }

    /// <summary>
    /// Gets or sets the maximum supported version of the custom
    /// Dave protocol that the client can use for this session.
    /// </summary>
    [JsonPropertyName("max_dave_protocol_version")]
    public required int MaxSupportedDaveVersion { get; init; }
}
