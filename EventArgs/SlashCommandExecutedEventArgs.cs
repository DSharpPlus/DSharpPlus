using Emzi0767.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace DSharpPlus.SlashCommands.EventArgs
{
    /// <summary>
    /// Represents the arguments for a <see cref="SlashCommandsExtension.SlashCommandExecuted"/> event
    /// </summary>
    public class SlashCommandExecutedEventArgs : AsyncEventArgs
    {
        /// <summary>
        /// The context of the command.
        /// </summary>
        public InteractionContext Context { get; internal set; }
    }
}