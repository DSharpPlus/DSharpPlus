using System;
using System.Collections.Generic;
using System.Text;

namespace DSharpPlus.SlashCommands.Attributes
{
    /// <summary>
    /// Marks this method as a slash command
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class SlashCommandAttribute : Attribute
    {
        /// <summary>
        /// Gets the name of this command
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the description of this command
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Marks this method as a slash command
        /// </summary>
        /// <param name="name">The name of this slash command</param>
        /// <param name="description">The description of this slash command</param>
        public SlashCommandAttribute(string name, string description)
        {
            Name = name;
            Description = description;
        }
    }
}