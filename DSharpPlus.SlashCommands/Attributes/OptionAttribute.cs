using System;

namespace DSharpPlus.SlashCommands
{
    /// <summary>
    /// Marks this parameter as an option for a slash command.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public sealed class OptionAttribute : Attribute
    {
        /// <summary>
        /// Gets the name of this option.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the description of this option.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Marks this parameter as an option for a slash command.
        /// </summary>
        /// <param name="name">The name of the option.</param>
        /// <param name="description">The description of the option.</param>
        public OptionAttribute(string name, string description)
        {
            if(name.Length > 32)
                throw new ArgumentException("Slash command option names cannot go over 32 characters.");
            if (description.Length > 100)
                throw new ArgumentException("Slash command option descriptions cannot go over 100 characters.");
            this.Name = name.ToLower();
            this.Description = description;
        }
    }
}
