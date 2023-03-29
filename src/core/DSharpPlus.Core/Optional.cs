using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DSharpPlus.Core;

/// <summary>
/// Represents an optional value - that is, a value that may either be present or not be present.
/// <see langword="null"/> is a valid presence.
/// </summary>
/// <typeparam name="T">Any parameter type.</typeparam>
public record struct Optional<T> : IOptional
{
    /// <summary>
    /// Gets an empty <see cref="Optional{T}"/>, with no provided value and indicating that no value will be provided.
    /// </summary>
    public static Optional<T> Empty { get; } = default;

    private T _value = default!;

    /// <summary>
    /// Gets or sets the underlying value of this instance.
    /// </summary>
    public T Value
    {
        get => HasValue ? _value : throw new InvalidOperationException("This Optional instance has no value.");
        set
        {
            _value = value;
            HasValue = true;
        }
    }

    /// <summary>
    /// Specifies whether this instance represents a value.
    /// </summary>
    public bool HasValue { get; internal set; } = false;

    /// <summary>
    /// Specifies whether this instance represents a value that is not null.
    /// </summary>
    public bool IsDefined => HasValue && _value is not null;

    /// <summary>
    /// Resolves the value from an optional value, if available
    /// </summary>
    /// <param name="value">The resolved value. This should only be utilized if the method returned true.</param>
    /// <returns>Whether this instance represents a non-null value.</returns>
    public bool Resolve
    (
        [NotNullWhen(true)]
        out T? value
    )
    {
        if(IsDefined)
        {
            value = Value!;
            return true;
        }
        else
        {
            value = default;
            return false;
        }
    }

    public static implicit operator T(Optional<T> parameter)
        => parameter.Value;

    public static implicit operator Optional<T>(T value)
        => new() { Value = value, HasValue = true };

    public Optional(T value)
    {
        Value = value;
        HasValue = true;
    }

    public static bool operator ==(Optional<T> optional, T value)
        => optional.HasValue && EqualityComparer<T>.Default.Equals(optional.Value, value);

    public static bool operator !=(Optional<T> optional, T value)
        => !(optional == value);

    public override int GetHashCode()
        => Value?.GetHashCode() ?? 0;

    public override string ToString()
        => HasValue ? Value?.ToString() ?? "null" : "Optional/no value";
}
