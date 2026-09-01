namespace DSharpPlus;

/// <summary>
/// Represents a result error returned when a timeout was exceeded.
/// </summary>
public sealed record TimeoutError : ResultError
{
    /// <summary>
    /// Creates a new timeout error.
    /// </summary>
    public TimeoutError() : base("Timed out.")
    {
        
    }
}
