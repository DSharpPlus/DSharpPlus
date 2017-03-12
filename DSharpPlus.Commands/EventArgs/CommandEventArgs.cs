using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DSharpPlus.Commands
{
    public class CommandEventArgs : EventArgs
    {
        public DiscordMessage Message { get; }
        public Command Command { get; }
        public IReadOnlyList<string> Arguments { get; }

        public DiscordChannel Channel => this.Message.Parent;
        public DiscordGuild Guild => this.Channel.Parent;
        public DiscordUser Auhtor => this.Message.Author;

        public CommandEventArgs(DiscordMessage message, Command command)
        {
            Message = message;
            Command = command;
            if (message.Content.Length > (CommandModule.instance.config.Prefix.Length + command.Name.Length))
            {
                string args = message.Content.Substring(CommandModule.instance.config.Prefix.Length + command.Name.Length + 1);
                Arguments = new ReadOnlyCollection<string>(args.Split(new char[] { ' ' }));
            }
        }
    }
}
