using System;
using DSharpPlus.Commands.ContextChecks.ParameterChecks;

namespace DSharpPlus.Commands.ArgumentModifiers;

/// <summary>
/// Determines the minimum and maximum values that a parameter can accept.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public sealed class MinMaxValueAttribute : ParameterCheckAttribute
{
    /// <summary>
    /// The minimum value that this parameter can accept.
    /// </summary>
    public object? MinValue { get; private init; }

    /// <summary>
    /// The maximum value that this parameter can accept.
    /// </summary>
    public object? MaxValue { get; private init; }

    /// <summary>
    /// Determines the minimum and maximum values that a parameter can accept.
    /// </summary>
    public MinMaxValueAttribute(object? minValue = null, object? maxValue = null)
    {
        this.MinValue = minValue;
        this.MaxValue = maxValue;

        if (minValue is not null && maxValue is not null && minValue.GetType() != maxValue.GetType())
        {
            throw new ArgumentException("The minimum and maximum values must be of the same type.");
        }

        if (minValue is null || maxValue is null)
        {
            return;
        }

        bool correctlyOrdered = minValue switch
        {
            byte => (byte)minValue <= (byte)maxValue,
            sbyte => (sbyte)minValue <= (sbyte)maxValue,
            short => (short)minValue <= (short)maxValue,
            ushort => (ushort)minValue <= (ushort)maxValue,
            int => (int)minValue <= (int)maxValue,
            uint => (uint)minValue <= (uint)maxValue,
            long => (long)minValue <= (long)maxValue,
            ulong => (ulong)minValue <= (ulong)maxValue,
            float => (float)minValue <= (float)maxValue,
            double => (double)minValue <= (double)maxValue,
            _ => throw new ArgumentException("The type of the minimum/maximum values is not supported."),
        };

        if (!correctlyOrdered)
        {
            throw new ArgumentException("The minimum value cannot be greater than the maximum value.");
        }
    }
}
