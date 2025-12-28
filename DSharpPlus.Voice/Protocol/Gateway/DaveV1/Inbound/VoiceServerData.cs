using System.Text.Json.Serialization;

/// <summary>
/// Represents the data included in a <c>VOICE_SERVER_UPDATE</c> event.
/// This provides the token, guild, and endpoint details required
/// to establish a Discord voice connection.
/// </summary>
public class VoiceServerData
{
    /// <summary>
    /// Gets or sets the voice connection token.
    /// This token is required to authenticate with the voice server.
    /// </summary>
    [JsonPropertyName("token")]
    public string Token { get; set; }

    /// <summary>
    /// Gets or sets the ID of the guild where the voice session
    /// is being established.
    /// </summary>
    [JsonPropertyName("guild_id")]
    public string GuildId { get; set; }

    /// <summary>
    /// Gets or sets the voice server endpoint.
    /// This is the hostname to which the client should connect.
    /// </summary>
    [JsonPropertyName("endpoint")]
    public string Endpoint { get; set; }
}
