namespace DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;

using System;

/// <summary>
/// Determines the minimum and maximum values that a parameter can accept.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public sealed class SlashMinMaxValueAttribute : Attribute
{
    /// <summary>
    /// The minimum value that this parameter can accept.
    /// </summary>
    public object? MinValue { get; init; }

    /// <summary>
    /// The maximum value that this parameter can accept.
    /// </summary>
    public object? MaxValue { get; init; }

    /// <summary>
    /// Determines the minimum and maximum values that a parameter can accept.
    /// </summary>
    public SlashMinMaxValueAttribute()
    {
        if (this.MinValue is int minInt && this.MaxValue is int maxInt && minInt > maxInt)
        {
            throw new ArgumentException("The minimum value cannot be greater than the maximum value.");
        }
        else if (this.MinValue is double minDouble && this.MaxValue is double maxDouble && minDouble > maxDouble)
        {
            throw new ArgumentException("The minimum value cannot be greater than the maximum value.");
        }
        else if (this.MinValue is not null && this.MaxValue is not null && this.MinValue.GetType() != this.MaxValue.GetType())
        {
            throw new ArgumentException("The minimum and maximum values must be of the same type.");
        }
    }
}
