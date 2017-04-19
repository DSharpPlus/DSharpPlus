using System;

namespace DSharpPlus.CommandsNext.Attributes
{
    /// <summary>
    /// Marks this method as a command.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class CommandAttribute : Attribute
    {
        /// <summary>
        /// Gets the name of this command.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Marks this method as a command with specified name.
        /// </summary>
        /// <param name="name">Name of this command.</param>
        public CommandAttribute(string name)
        {
            this.Name = name;
        }
    }
}
