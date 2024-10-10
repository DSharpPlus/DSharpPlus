namespace DSharpPlus.Entities;


/// <summary>
/// Represents the type of a message reference.
/// </summary>
public enum DiscordMessagReferenceType : int
{
    /// <summary>
    /// A standard reference used by replies.
    /// </summary>
    Default = 0,

    /// <summary>
    /// Reference used to point to a message at a point in time.
    /// </summary>
    Forward = 1,
}
