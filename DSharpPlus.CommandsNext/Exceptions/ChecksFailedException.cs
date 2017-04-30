using System;

namespace DSharpPlus.CommandsNext.Exceptions
{
    /// <summary>
    /// Indicates that one or more checks for given command have failed.
    /// </summary>
    public class ChecksFailedException : Exception
    {
        /// <summary>
        /// Gets the command that was executed.
        /// </summary>
        public Command Command { get; }

        /// <summary>
        /// Gets the context in which given command was executed.
        /// </summary>
        public CommandContext Context { get; }

        /// <summary>
        /// Creates a new <see cref="ChecksFailedException"/>.
        /// </summary>
        /// <param name="message">Message that describes the error.</param>
        /// <param name="command">Command that failed to execute.</param>
        /// <param name="ctx">Context in which the command was executed.</param>
        internal ChecksFailedException(string message, Command command, CommandContext ctx)
            : base(message)
        {
            this.Command = command;
            this.Context = ctx;
        }
    }
}
