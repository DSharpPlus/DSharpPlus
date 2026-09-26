namespace DSharpPlus.Interactivity.InteractiveMessages;

/// <summary>
/// Represents options of how to handle an interactive message timing out.
/// </summary>
public enum TimeoutBehaviour
{
    /// <summary>
    /// The components should be disabled when the message times out.
    /// </summary>
    Disable,

    /// <summary>
    /// The components should be left as is when the message times out.
    /// </summary>
    Ignore,

    /// <summary>
    /// The entire message should be deleted when the message times out.
    /// </summary>
    DeleteMessage,
}
