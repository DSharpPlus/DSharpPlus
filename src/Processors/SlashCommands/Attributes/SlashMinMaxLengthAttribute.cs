using System;

namespace DSharpPlus.CommandAll.Processors.SlashCommands.Attributes
{
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
            if (MinLength is not null && MaxLength is not null && MinLength > MaxLength)
            {
                throw new ArgumentException("The minimum length cannot be greater than the maximum length.");
            }
        }
    }
}
