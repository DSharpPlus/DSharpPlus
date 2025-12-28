namespace DSharpPlus.Voice;

/// <summary>
/// Represents the known data of a user in a voice connection.
/// </summary>
public sealed record VoiceUser
{
    /// <summary>
    /// The snowflake ID of this user.
    /// </summary>
    public ulong UserId { get; init; }

    /// <summary>
    /// Indicates whether this user is currently speaking.
    /// </summary>
    public bool IsSpeaking { get; internal set; }

    /// <summary>
    /// The SSRC of this user, if one has been specified.
    /// </summary>
    public int? Ssrc { get; internal set; }
}
