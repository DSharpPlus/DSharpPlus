using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Objects;

namespace DSharpPlus.Commands
{
    public class DiscordCommandEventArgs
    {
        public string[] args;

        public DiscordCommand Command;
        public DiscordMessage Message;

        public DiscordMember Member => Message.Author;
        public DiscordChannel Channel => Message.Channel();
        public DiscordServer Server => Message.Channel().Server;

        public string GetArg(int index) => args[index];
    }
}
