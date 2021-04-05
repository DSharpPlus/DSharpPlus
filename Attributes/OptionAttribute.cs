using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSharpPlus.SlashCommands.Attributes
{
    /// <summary>
    /// Marks this parameter as an option for a slash command
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class OptionAttribute : Attribute
    {
        /// <summary>
        /// Gets the name of this option
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the description of this option
        /// </summary>
        public string Description;

        /// <summary>
        /// Marks this parameter as an option for a slash command
        /// </summary>
        /// <param name="name">The name of the option</param>
        /// <param name="description">The description of the option</param>
        public OptionAttribute(string name, string description)
        {
            Name = name;
            Description = description;
        }
    }
}