namespace DSharpPlus.Interactivity.Enums;


/// <summary>
/// Specifies what should be done once pagination times out.
/// </summary>
public enum PaginationDeletion
{
    /// <summary>
    /// Reaction emojis will be deleted on timeout.
    /// </summary>
    DeleteEmojis = 0,

    /// <summary>
    /// Reaction emojis will not be deleted on timeout.
    /// </summary>
    KeepEmojis = 1,

    /// <summary>
    /// The message will be completely deleted on timeout.
    /// </summary>
    DeleteMessage = 2
}
