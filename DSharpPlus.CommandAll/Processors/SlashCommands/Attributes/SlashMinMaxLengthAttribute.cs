namespace DSharpPlus.CommandAll.Processors.SlashCommands.Attributes;

using System;

/// <summary>
/// Determines the minimum and maximum length that a parameter can accept.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public sealed class SlashMinMaxLengthAttribute : Attribute
{
    /// <summary>
    /// The minimum length that this parameter can accept.
    /// </summary>
    public int? MinLength { get; init; }

    /// <summary>
    /// The maximum length that this parameter can accept.
    /// </summary>
    public int? MaxLength { get; init; }

    /// <summary>
    /// Determines the minimum and maximum length that a parameter can accept.
    /// </summary>
    public SlashMinMaxLengthAttribute()
    {
        if (this.MinLength is not null && this.MaxLength is not null && this.MinLength > this.MaxLength)
        {
            throw new ArgumentException("The minimum length cannot be greater than the maximum length.");
        }
    }
}
