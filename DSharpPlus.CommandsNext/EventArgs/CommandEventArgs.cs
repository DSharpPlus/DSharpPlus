using System;

namespace DSharpPlus.CommandsNext
{
    public class CommandEventArgs : EventArgs
    {
        public CommandContext Context { get; internal set; }
        public Command Command => this.Context.Command;
    }
}
