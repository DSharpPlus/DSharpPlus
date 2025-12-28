using System.Text.Json.Serialization;

/// <summary>
/// Represents the data included in a voice identify payload.
/// This data is sent by the client to the Discord voice server
/// to authenticate and establish a new voice connection.
/// </summary>
public class VoiceIdentifyData
{
    /// <summary>
    /// Gets or sets the ID of the server (guild) that the
    /// client is attempting to connect to.
    /// </summary>
    [JsonPropertyName("server_id")]
    public string ServerId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the user who is identifying
    /// with the voice server.
    /// </summary>
    [JsonPropertyName("user_id")]
    public string UserId { get; set; }

    /// <summary>
    /// Gets or sets the session ID of the voice connection,
    /// provided by the Gateway during the initial setup.
    /// </summary>
    [JsonPropertyName("session_id")]
    public string SessionId { get; set; }

    /// <summary>
    /// Gets or sets the authentication token used to validate
    /// the voice connection request.
    /// </summary>
    [JsonPropertyName("token")]
    public string Token { get; set; }

    /// <summary>
    /// Gets or sets the maximum supported version of the custom
    /// Dave protocol that the client can use for this session.
    /// </summary>
    [JsonPropertyName("max_dave_protocol_version")]
    public int MaxSupportedDaveVersion { get; set; }
}
