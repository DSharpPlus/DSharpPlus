using System;

namespace DSharpPlus.CommandsNext.Exceptions
{
    /// <summary>
    /// Indicates that given command name or alias is taken.
    /// </summary>
    public class CommandExistsException : Exception
    {
        /// <summary>
        /// Gets the name of the command that already exists.
        /// </summary>
        public string CommandName { get; }

        /// <summary>
        /// Creates a new exception indicating that given command name is already taken.
        /// </summary>
        /// <param name="message">Message that describes the error.</param>
        /// <param name="name">Name of the command that was taken.</param>
        internal CommandExistsException(string message, string name)
            : base(message)
        {
            this.CommandName = name;
        }
    }
}
