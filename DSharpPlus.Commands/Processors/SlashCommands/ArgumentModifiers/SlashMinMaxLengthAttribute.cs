using System;

namespace DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;

/// <summary>
/// Determines the minimum and maximum length that a parameter can accept.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public sealed class SlashMinMaxLengthAttribute : Attribute
{
    private const int MinLengthMinimum = 0;
    private const int MinLengthMaximum = 6000;
    private const int MaxLengthMinimum = 1;
    private const int MaxLengthMaximum = 6000;

    /// <summary>
    /// The minimum length that this parameter can accept.
    /// </summary>
    public int MinLength { get; init; } = MinLengthMinimum;

    /// <summary>
    /// The maximum length that this parameter can accept.
    /// </summary>
    public int MaxLength { get; init; } = MaxLengthMaximum;

    /// <summary>
    /// Determines the minimum and maximum length that a parameter can accept.
    /// </summary>
    public SlashMinMaxLengthAttribute()
    {
        if (this.MinLength is < MinLengthMinimum or > MinLengthMaximum)
        {
            throw new ArgumentException($"The minimum length cannot be less than {MinLengthMinimum} and greater than {MinLengthMaximum}.");
        }

        if (this.MaxLength is < MaxLengthMinimum or > MaxLengthMaximum)
        {
            throw new ArgumentException($"The maximum length cannot be less than {MaxLengthMinimum} and greater than {MaxLengthMaximum}.");
        }

        if (this.MinLength > this.MaxLength)
        {
            throw new ArgumentException("The minimum length cannot be greater than the maximum length.");
        }
    }
}
