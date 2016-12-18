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
        public string[] Arguments { get; }

        public CommandEventArgs(DiscordMessage message, Command command)
        {
            Message = message;
            Command = command;
            if (message.Content.Length > (CommandModule.instance.config.Prefix.Length + command.Name.Length))
            {
                string args = message.Content.Substring(CommandModule.instance.config.Prefix.Length + command.Name.Length + 1);
                Arguments = args.Split(new char[] { ' ' });
            }
        }
    }
}
