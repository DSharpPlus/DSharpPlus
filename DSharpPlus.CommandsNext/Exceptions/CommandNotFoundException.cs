﻿using System;

namespace DSharpPlus.CommandsNext.Exceptions
{
    /// <summary>
    /// Thrown when the command service fails to find a command.
    /// </summary>
    public sealed class CommandNotFoundException : Exception
    {
        /// <summary>
        /// Gets the name of the command that was not found.
        /// </summary>
        public string Command { get; set; }

        /// <summary>
        /// Creates a new <see cref="CommandNotFoundException"/>.
        /// </summary>
        /// <param name="message">Message that describes the error.</param>
        /// <param name="command">Command that was not found.</param>
        public CommandNotFoundException(string message, string command)
            : base(message)
        {
            this.Command = command;
        }
    }
}
