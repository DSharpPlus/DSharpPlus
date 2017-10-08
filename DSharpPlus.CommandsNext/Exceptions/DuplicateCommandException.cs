using System;

namespace DSharpPlus.CommandsNext.Exceptions
{
    /// <summary>
    /// Indicates that given command name or alias is taken.
    /// </summary>
    public class DuplicateCommandException : Exception
    {
        /// <summary>
        /// Gets the name of the command that already exists.
        /// </summary>
        public string CommandName { get; }

        /// <summary>
        /// Creates a new exception indicating that given command name is already taken.
        /// </summary>
        /// <param name="name">Name of the command that was taken.</param>
        public DuplicateCommandException(string name)
            : base("A command with a given name already exists.")
        {
            this.CommandName = name;
        }
    }
}
