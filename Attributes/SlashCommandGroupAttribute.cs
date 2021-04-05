using System;
using System.Collections.Generic;
using System.Text;

namespace DSharpPlus.SlashCommands.Attributes
{
    /// <summary>
    /// Marks this class a slash command group
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class SlashCommandGroupAttribute : Attribute
    {
        /// <summary>
        /// Gets the name of this slash command group
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the description of this slash command group
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Marks this class as a slash command group
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        public SlashCommandGroupAttribute(string name, string description)
        {
            Name = name;
            Description = description;
        }
    }
}