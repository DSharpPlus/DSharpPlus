using System;
using System.Threading;
using System.Threading.Tasks;

namespace DSharpPlus.Commands
{
    public class Command
    {
        public string Name { get; set; } = "";
        public Func<CommandEventArgs, Task> Func { get; set; } = null;

        public Command(string command, Func<CommandEventArgs, Task> func)
        {
            Name = command;
            Func = func;
        }

        public void Execute(CommandEventArgs args)
        {
            ThreadPool.QueueUserWorkItem(o => Func(args).GetAwaiter().GetResult());
        }
    }
}
