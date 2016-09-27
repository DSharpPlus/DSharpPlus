using DSharpPlus.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSharpPlus.Testing
{
    class Command_Greet : DiscordCommand
    {
        public Command_Greet() : base()
        {
            Keyword = "greet";
            Aliases.Add("hello");
            SetInvokeFunction(new Action<DiscordCommandEventArgs>(RunCommand));
        }

        public void RunCommand(DiscordCommandEventArgs e)
        {
            e.Channel.SendMessage("Hello, " + e.Member.Mention + "!");
        }
    }
}
