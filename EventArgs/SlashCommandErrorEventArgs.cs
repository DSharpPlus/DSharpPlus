using Emzi0767.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace DSharpPlus.SlashCommands.EventArgs
{
    /// <summary>
    /// Represents arguments for a <see cref="SlashCommandsExtension.SlashCommandErrored"/> event
    /// </summary>
    public class SlashCommandErrorEventArgs : AsyncEventArgs
    {
        /// <summary>
        /// The context of the command.
        /// </summary>
        public InteractionContext Context { get; internal set; }

        /// <summary>
        /// The exception thrown.
        /// </summary>
        public Exception Exception { get; internal set; }
    }
}