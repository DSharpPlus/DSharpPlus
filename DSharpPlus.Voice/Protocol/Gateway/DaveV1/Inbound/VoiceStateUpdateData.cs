
using System.Text.Json.Serialization;

public class VoiceStateUpdateData
{
    /// <summary>
    /// Gets/Sets the guild id
    /// </summary>
    [JsonPropertyName("guild_id")]
    public string GuildId { get; set; }
    /// <summary>
    /// Gets/Sets the channel id
    /// </summary>
    [JsonPropertyName("channel_id")]
    public string ChannelId { get; set; }
    /// <summary>
    /// Gets/Sets the users id
    /// </summary>
    [JsonPropertyName("user_id")]
    public string UserId { get; set; }
    /// <summary>
    /// Gets/Sets the session id
    /// </summary>
    [JsonPropertyName("session_id")]
    public string SessionId { get; set; }
    /// <summary>
    /// Gets/Sets if the event is supressed
    /// </summary>
    [JsonPropertyName("suppress")]
    public bool Suppress { get; set; }
    /// <summary>
    /// Gets/Sets if the user has video enabled
    /// </summary>
    [JsonPropertyName("self_video")]
    public bool SelfVideo { get; set; }
    /// <summary>
    /// Gets/Sets if the user is self muted
    /// </summary>
    [JsonPropertyName("self_mute")]
    public bool SelfMute { get; set; }
    /// <summary>
    /// Gets/Sets if the user is self deafened
    /// </summary>
    [JsonPropertyName("self_deaf")]
    public bool SelfDeaf { get; set; }
    /// <summary>
    /// Gets/Sets if the user is server muted
    /// </summary>
    [JsonPropertyName("mute")]
    public bool Mute { get; set; }

    /// <summary>
    /// Gets/Sets if the user is server deafened
    /// </summary>
    [JsonPropertyName("deaf")]
    public bool Deaf { get; set; }

    /// <summary>
    /// Gets/Sets the request to speak timestamp
    /// </summary>
    [JsonPropertyName("request_to_speak_timestamp")]
    public string RequestToSpeakTimestamp { get; set; }

    /// <summary>
    /// Gets/Sets the member data of the user
    /// </summary>
    [JsonPropertyName("member")]
    public VoiceStateMember Member { get; set; }
}
