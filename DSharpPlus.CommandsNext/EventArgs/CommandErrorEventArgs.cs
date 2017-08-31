using System;

namespace DSharpPlus.CommandsNext
{
    /// <summary>
    /// Represents arguments for <see cref="CommandsNextModule.CommandErrored"/> event.
    /// </summary>
    public class CommandErrorEventArgs : CommandEventArgs
    {
        public Exception Exception { get; internal set; }
    }
}
