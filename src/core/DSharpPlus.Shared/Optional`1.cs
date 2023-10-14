// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Diagnostics.CodeAnalysis;

namespace DSharpPlus;

/// <summary>
/// Represents a logical container for the presence of a value in the context of Discord's REST API.
/// </summary>
/// <typeparam name="T">The type of the enclosed value.</typeparam>
public readonly record struct Optional<T> : IOptional
{
    private readonly T? value;

    /// <inheritdoc/>
    public bool HasValue { get; }

    /// <summary>
    /// Retrieves the underlying value, if present.
    /// </summary>
    public T? Value
    {
        get
        {
            if (!this.HasValue)
            {
                ThrowHelper.ThrowOptionalNoValuePresent();
            }

            return this.value;
        }
    }

    public Optional
    (
        T? value
    )
    {
        this.HasValue = true;
        this.value = value;
    }

    /// <summary>
    /// Returns the contained value if one is present, or throws the given exception if none is present.
    /// </summary>
    public readonly T? Expect
    (
        Func<Exception> exception
    )
    {
        if (!this.HasValue)
        {
            ThrowHelper.ThrowFunc(exception);
        }

        return this.value;
    }

    /// <summary>
    /// Returns the contained value if present, or the provided value if not present.
    /// </summary>
    public readonly T? Or(T value) 
        => this.HasValue ? this.value : value;

    /// <summary>
    /// Returns the contained value if present, or the default value for this type if not present.
    /// </summary>
    public readonly T? OrDefault()
        => this.HasValue ? this.value : default;

    /// <summary>
    /// Transforms the given optional to an optional of <typeparamref name="TOther"/> if it has a value,
    /// returning an empty optional if there was no value present.
    /// </summary>
    public readonly Optional<TOther> Map<TOther>
    (
        Func<T?, TOther?> transformation
    )
    {
        return this.HasValue
            ? new Optional<TOther>(transformation(this.value))
            : new Optional<TOther>();
    }

    /// <summary>
    /// Transforms the value of the given optional to <typeparamref name="TOther"/>, returning
    /// <paramref name="value"/> if there was no value present.
    /// </summary>
    public readonly TOther? MapOr<TOther>
    (
        Func<T?, TOther?> transformation,
        TOther? value
    )
    {
        return this.HasValue
            ? transformation(this.value)
            : value;
    }

    /// <summary>
    /// Returns a value indicating whether <paramref name="value"/> is set.
    /// </summary>
    /// <param name="value">The value of this optional. This may still be null if the set value was null.</param>
    public readonly bool TryGetValue
    (
        out T? value
    )
    {
        if (this.HasValue)
        {
            value = this.value;
            return true;
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Returns a value indicating whether <paramref name="value"/> is set and not null.
    /// </summary>
    /// <param name="value">The value of this optional.</param>
    [MemberNotNullWhen(true, nameof(value))]
    public readonly bool TryGetNonNullValue
    (
        [NotNullWhen(true)]
        out T? value
    )
    {
        if (this.HasValue && this.value is not null)
        {
            value = this.value;
            return true;
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Returns a string representing the present optional instance.
    /// </summary>
    public override string ToString()
    {
        return this.HasValue
            ? $"Optional {{ {this.value} }}"
            : "Optional { no value }";
    }

    public static implicit operator Optional<T>(T value)
        => new(value);
}
