using System;

namespace DSharpPlus.CommandsNext
{
    public class CommandErrorEventArgs : CommandEventArgs
    {
        public Exception Exception { get; internal set; }
    }
}
