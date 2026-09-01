namespace DSharpPlus.Interactivity.InteractiveMessages;

/// <summary>
/// Represents the current state of user interactions with a given message.
/// </summary>
public readonly record struct InteractiveMessageState
{
    /// <summary>
    /// Indicates that further interactivity on this message is disabled.
    /// </summary>
    public bool IsDisabled { get; init; }

    /// <summary>
    /// The page this message currently shows, if it is paginated.
    /// </summary>
    public int Page { get; init; }
}
