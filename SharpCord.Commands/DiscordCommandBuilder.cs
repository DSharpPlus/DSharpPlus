using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpCord;

namespace SharpCord.Commands
{
    public class DiscordCommandBuilder
    {
        private DiscordCommand _command;

        public DiscordCommandBuilder AddAlias(params string[] aliases)
        {
            _command.Aliases.AddRange(aliases);
            return this;
        }

        public DiscordCommandBuilder SetDescription(string desc)
        {
            _command.Description = desc;
            return this;
        }

        public DiscordCommandBuilder AddParameter(DiscordCommandParameter param)
        {
            if (_command.ContainsParameterOfType(DiscordCommandParameterType.Multiple))
                throw new System.Exception("No parameters may be added after a parameter with a type of 'Multiple'.");

            _command.Parameters.Add(param);

            return this;
        }

        public DiscordCommandBuilder LockToDevelopers()
        {
            _command.OnlyForDevelopers = true;
            return this;
        }

        public DiscordCommandBuilder Do(Func<DiscordCommandEventArgs, Task> invokeFunction)
        {
            _command.SetInvokeFunction(invokeFunction);
            return this;
        }

        public DiscordCommandBuilder Do(Action<DiscordCommandEventArgs> invokeFunction)
        {
            _command.SetInvokeFunction(invokeFunction);
            return this;
        }
    }
}
