namespace DSharpPlus.Interactivity;

/// <summary>
/// Represents the target message having been deleted during an interactivity operation.
/// </summary>
public sealed record MessageDeletedError : ResultError
{
    /// <summary>
    /// Creates a new instance of this result error.
    /// </summary>
    public MessageDeletedError(string message) : base(message)
    {
    }
}
