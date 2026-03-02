using System.Diagnostics.CodeAnalysis;

using DSharpPlus.EventArgs;

namespace DSharpPlus.Clients;

// really, this should just be a result type
/// <summary>
/// Represents either the received event or an indication that the event waiter timed out.
/// </summary>
public readonly record struct EventWaiterResult<T>
    where T : DiscordEventArgs
{
    /// <summary>
    /// Indicates that this event waiter timed out.
    /// </summary>
    public bool TimedOut { get; init; }

    /// <summary>
    /// The event this event waiter sought to receive.
    /// </summary>
    [MemberNotNullWhen(false, nameof(TimedOut))]
    public T? Value { get; init; }

    /// <summary>
    /// Creates a new result that indicates having timed out.
    /// </summary>
    public static EventWaiterResult<T> FromTimedOut()
        => new() { TimedOut = true };
}