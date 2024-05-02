namespace DSharpPlus.Interactivity.Enums;


/// <summary>
/// Specifies what should be done when a poll times out.
/// </summary>
public enum PollBehaviour
{
    /// <summary>
    /// Reaction emojis will not be deleted.
    /// </summary>
    KeepEmojis = 0,

    /// <summary>
    /// Reaction emojis will be deleted.
    /// </summary>
    DeleteEmojis = 1
}
