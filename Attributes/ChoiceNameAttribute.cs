using System;

namespace DSharpPlus.SlashCommands
{
    /// <summary>
    /// Sets the name for this enum choice.
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    public class ChoiceNameAttribute : Attribute
    {
        /// <summary>
        /// The name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Sets the name for this enum choice.
        /// </summary>
        /// <param name="name">The name for this enum choice.</param>
        public ChoiceNameAttribute(string name)
        {
            Name = name;
        }
    }
}
