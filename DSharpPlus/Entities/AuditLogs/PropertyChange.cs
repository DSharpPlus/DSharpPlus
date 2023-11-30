using DSharpPlus.Net.Abstractions;

namespace DSharpPlus.Entities.AuditLogs;

using System;

/// <summary>
/// Represents a description of how a property changed.
/// </summary>
/// <typeparam name="T">Type of the changed property.</typeparam>
public readonly record struct PropertyChange<T>
{
    /// <summary>
    /// The property's value before it was changed.
    /// </summary>
    public Optional<T> Before { get; internal init; }

    /// <summary>
    /// The property's value after it was changed.
    /// </summary>
    public Optional<T> After { get; internal init; }

    /// <summary>
    /// Create a new <see cref="PropertyChange{T}"/> from the given before and after values.
    /// </summary>
    public static PropertyChange<T> From(T? before, T? after) =>
        new()
        {
            Before = before is T ? (T)before : default(Optional<T>),
            After = after is T ? (T)after : default(Optional<T>)
        };

    /// <summary>
    /// Create a new <see cref="PropertyChange{T}"/> from the given change using before and after values.
    /// </summary>
    internal static PropertyChange<T> From(AuditLogActionChange change) =>
        From(change.OldValue, change.NewValue);
}
