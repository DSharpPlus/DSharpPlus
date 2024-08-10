using System;
using System.Collections;

namespace DSharpPlus.Commands.ArgumentModifiers;

/// <summary>
/// Specifies that a parameter can accept multiple arguments.
/// This attribute is only valid on parameters of type <see cref="IEnumerable"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
public sealed class MultiArgumentAttribute : Attribute
{
    /// <summary>
    /// The maximum number of arguments that this parameter can accept.
    /// </summary>
    public int MaximumArgumentCount { get; init; }

    /// <summary>
    /// The minimum number of arguments that this parameter can accept.
    /// </summary>
    public int MinimumArgumentCount { get; init; }

    /// <summary>
    /// The number of arguments that this parameter can accept.
    /// </summary>
    /// <param name="maximumArgumentCount">The maximum number of arguments that this parameter can accept.</param>
    /// <param name="minimumArgumentCount">The minimum number of arguments that this parameter can accept.</param>
    public MultiArgumentAttribute(int maximumArgumentCount, int minimumArgumentCount = 1)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(minimumArgumentCount, 1, nameof(minimumArgumentCount));
        ArgumentOutOfRangeException.ThrowIfLessThan(maximumArgumentCount, 1, nameof(maximumArgumentCount));
        ArgumentOutOfRangeException.ThrowIfLessThan(maximumArgumentCount, minimumArgumentCount, nameof(maximumArgumentCount));

        this.MaximumArgumentCount = maximumArgumentCount;
        this.MinimumArgumentCount = minimumArgumentCount;
    }
}
