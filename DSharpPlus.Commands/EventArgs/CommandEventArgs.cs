using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSharpPlus.Commands
{
    public class CommandEventArgs : EventArgs
    {
        public DiscordMessage Message { get; }
        public Command Command { get; }

        public CommandEventArgs(DiscordMessage message, Command command)
        {
            Message = message;
            Command = command;
        }
    }
}
