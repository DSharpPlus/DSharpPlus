/// <summary>
/// Represents the known data of a user in our voice call
/// </summary>
public class UserInVoice
{
    /// <summary>
    /// The users snowflake id
    /// </summary>
    public ulong UserId { get; set; }
    /// <summary>
    /// If the user is currently speaking or not
    /// </summary>
    public bool IsSpeaking { get; set; }
    /// <summary>
    /// The users ssrc, this can be null as we only know 
    /// what this value is after their first time speaking
    /// </summary>
    public int? Ssrc { get; set; }
}
