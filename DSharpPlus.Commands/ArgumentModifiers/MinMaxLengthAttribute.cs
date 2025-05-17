using System;
using DSharpPlus.Commands.ContextChecks.ParameterChecks;

namespace DSharpPlus.Commands.ArgumentModifiers;

/// <summary>
/// Determines the minimum and maximum length that a parameter can accept.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public sealed class MinMaxLengthAttribute : ParameterCheckAttribute
{
    // on text commands, we interpret 6000 as unlimited - it exceeds the message limit anyway
    private const int MinLengthMinimum = 0;
    private const int MinLengthMaximum = 6000;
    private const int MaxLengthMinimum = 1;
    private const int MaxLengthMaximum = 6000;

    /// <summary>
    /// The minimum length that this parameter can accept.
    /// </summary>
    public int MinLength { get; private init; }

    /// <summary>
    /// The maximum length that this parameter can accept.
    /// </summary>
    public int MaxLength { get; private init; }

    /// <summary>
    /// Determines the minimum and maximum length that a parameter can accept.
    /// </summary>
    public MinMaxLengthAttribute(int minLength = MinLengthMinimum, int maxLength = MaxLengthMaximum)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(minLength, MinLengthMinimum, nameof(minLength));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(minLength, MinLengthMaximum, nameof(minLength));
        ArgumentOutOfRangeException.ThrowIfLessThan(maxLength, MaxLengthMinimum, nameof(maxLength));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(maxLength, MaxLengthMaximum, nameof(maxLength));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(minLength, maxLength, nameof(minLength));

        this.MinLength = minLength;
        this.MaxLength = maxLength;
    }
}
