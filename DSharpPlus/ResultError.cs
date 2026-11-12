namespace DSharpPlus;

/// <summary>
/// Represents an error that has occurred during the operation described by the Result containing this error.
/// </summary>
public abstract record ResultError
{
    /// <summary>
    /// The human-readable error message.
    /// </summary>
    public string Message { get; init; }

    protected ResultError(string message)
        => this.Message = message;
}
