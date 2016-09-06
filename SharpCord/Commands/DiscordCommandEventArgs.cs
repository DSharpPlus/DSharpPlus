using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpCord;
using SharpCord.Objects;

namespace SharpCord.Commands
{
    public class DiscordCommandEventArgs
    {
        public readonly string[] args;

        public DiscordCommand Command;
        public DiscordMessage Message;

        public DiscordMember Member => Message.Author;
        public DiscordChannel Channel => Message.Channel();
        public DiscordServer Server => Message.Channel().Server;

        public string GetArg(int index) => args[index];
    }
}
