namespace DSharpPlus.Interactivity;

/// <summary>
/// Interactivity result
/// </summary>
/// <typeparam name="T">Type of result</typeparam>
public readonly struct InteractivityResult<T>
{
    /// <summary>
    /// Whether interactivity was timed out
    /// </summary>
    public bool TimedOut { get; }
    /// <summary>
    /// Result
    /// </summary>
    public T Result { get; }

    internal InteractivityResult(bool timedout, T result)
    {
        this.TimedOut = timedout;
        this.Result = result;
    }
}
