using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;

namespace DSharpPlus.Commands
{
    public class DiscordCommandBuilder
    {
        public DiscordCommand Command;

        public DiscordCommandBuilder AddAlias(params string[] aliases)
        {
            Command.Aliases.AddRange(aliases);
            return this;
        }

        public DiscordCommandBuilder SetDescription(string desc)
        {
            Command.Description = desc;
            return this;
        }

        public DiscordCommandBuilder AddParameter(DiscordCommandParameter param)
        {
            if (Command.ContainsParameterOfType(DiscordCommandParameterType.Multiple))
                throw new System.Exception("No parameters may be added after a parameter with a type of 'Multiple'.");

            Command.Parameters.Add(param);

            return this;
        }

        public DiscordCommandBuilder AddParameter(string name, DiscordCommandParameterType type, string desc="")
        {
            Command.Parameters.Add(new DiscordCommandParameter(name, desc, type));
            return this;
        }

        public DiscordCommandBuilder LockToDevelopers()
        {
            Command.OnlyForDevelopers = true;
            return this;
        }

        public DiscordCommandBuilder Do(Func<DiscordCommandEventArgs, Task> invokeFunction)
        {
            Command.SetInvokeFunction(invokeFunction);
            return this;
        }

        public DiscordCommandBuilder Do(Action<DiscordCommandEventArgs> invokeFunction)
        {
            Command.SetInvokeFunction(invokeFunction);
            return this;
        }
    }
}
