using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpCord.Utility;

namespace SharpCord.Commands
{
    public class DiscordCommand
    {
        public string Keyword;
        public string Description;
        public List<string> Aliases;
        public bool OnlyForDevelopers = false;
        public List<DiscordCommandParameter> Parameters;
        private Func<DiscordCommandEventArgs, Task> InvokeFunction;

        public DiscordCommand(string keyword, string[] aliases = null)
        {
            this.Keyword = keyword;
            if (aliases != null)
                this.Aliases = aliases;
        }

        public void SetInvokeFunction(Func<DiscordCommandEventArgs, Task> _invokeFunction)
        {
            this.InvokeFunction = _invokeFunction;
        }

        public void SetInvokeFunction(Action<DiscordCommandEventArgs> _invokeFunction)
        {
            this.InvokeFunction = TaskHelper.ToAsync(_invokeFunction);
        }

        public Task Run(DiscordCommandEventArgs args)
        {
            var task = InvokeFunction(args);
            return (task == null ? TaskHelper.CompletedTask : task);
        }

        public bool ContainsParameterOfType(DiscordCommandParameterType type)
        {
            return Parameters.Where(t => t.parameterType == type).Count() > 0;
        }
    }
}
