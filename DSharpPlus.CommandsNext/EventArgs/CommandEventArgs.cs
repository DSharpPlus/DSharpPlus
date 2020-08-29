﻿using Emzi0767.Utilities;

namespace DSharpPlus.CommandsNext
{
    /// <summary>
    /// Base class for all CNext-related events.
    /// </summary>
    public class CommandEventArgs : AsyncEventArgs
    {
        /// <summary>
        /// Gets the context in which the command was executed.
        /// </summary>
        public CommandContext Context { get; internal set; }

        /// <summary>
        /// Gets the command that was executed.
        /// </summary>
        public Command Command 
            => this.Context.Command;
    }
}
