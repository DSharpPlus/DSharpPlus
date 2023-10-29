using System;

namespace DSharpPlus.CommandAll.Processors.SlashCommands.Attributes
{
    /// <summary>
    /// Determines the minimum and maximum values that a parameter can accept.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class SlashMinMaxAttribute : Attribute
    {
        /// <summary>
        /// The minimum value that this parameter can accept.
        /// </summary>
        public int? MinValue { get; init; }

        /// <summary>
        /// The maximum value that this parameter can accept.
        /// </summary>
        public int? MaxValue { get; init; }

        /// <summary>
        /// Determines the minimum and maximum values that a parameter can accept.
        /// </summary>
        public SlashMinMaxAttribute()
        {
            if (MinValue is int minInt && MaxValue is int maxInt && minInt > maxInt)
            {
                throw new ArgumentException("The minimum value cannot be greater than the maximum value.");
            }
            else if (MinValue is not null && MaxValue is not null && MinValue.GetType() != MaxValue.GetType())
            {
                throw new ArgumentException("The minimum and maximum values must be of the same type.");
            }
        }
    }
}
