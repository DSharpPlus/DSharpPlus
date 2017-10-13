﻿using System;

// ReSharper disable once CheckNamespace
namespace DSharpPlus.CommandsNext
{
    /// <summary>
    /// Represents a command's execution result.
    /// </summary>
    public struct CommandResult
    {
        /// <summary>
        /// Gets whether the command execution succeeded.
        /// </summary>
        public bool IsSuccessful { get; internal set; }
        
        /// <summary>
        /// Gets the exception (if any) that occured when executing the command.
        /// </summary>
        public Exception Exception { get; internal set; }

        /// <summary>
        /// Gets the context in which the command was executed.
        /// </summary>
        public CommandContext Context { get; internal set; }
    }
}
