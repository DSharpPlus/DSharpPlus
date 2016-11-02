using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Utility;

namespace DSharpPlus.Commands
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
            this.Parameters = new List<DiscordCommandParameter>();
			if (aliases != null)
				this.Aliases = new List<string>(aliases);
			else
				this.Aliases = new List<string>();
		}
        //This is horrible practice, but I'm trying to get this done so ;-;
        public DiscordCommand()
        {
            this.Parameters = new List<DiscordCommandParameter>();
            this.Aliases = new List<string>();
        }

        public static DiscordCommandBuilder Create(string keyword)
        {
            DiscordCommandBuilder b = new DiscordCommandBuilder();
            b.Command = new DiscordCommand(keyword);
            return b;
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
            if (Parameters == null)
                return false;
            foreach(var param in Parameters)
            {
                if (param.parameterType == type)
                    return true;
            }
            return false;
        }
    }
}
