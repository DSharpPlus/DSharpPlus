using DSharpPlus.Net.Abstractions;

namespace DSharpPlus.Entities.AuditLogs;

/// <summary>
/// Represents a description of how a property changed.
/// </summary>
/// <typeparam name="T">Type of the changed property.</typeparam>
public readonly record struct PropertyChange<T>
{
    /// <summary>
    /// The property's value before it was changed.
    /// </summary>
    public T Before { get; internal init; }

    /// <summary>
    /// The property's value after it was changed.
    /// </summary>
    public T After { get; internal init; }

    /// <summary>
    /// Create a new <see cref="PropertyChange{T}"/> from the given before and after values.
    /// </summary>
    public static PropertyChange<T> From(object before, object after) =>
        new()
        {
            Before = before is not null && before is T ? (T)before : default,
            After = after is not null && after is T ? (T)after : default
        };

    /// <summary>
    /// Create a new <see cref="PropertyChange{T}"/> from the given change using before and after values.
    /// </summary>
    internal static PropertyChange<T> From(AuditLogActionChange change) =>
        From(change.OldValue, change.NewValue);
}
