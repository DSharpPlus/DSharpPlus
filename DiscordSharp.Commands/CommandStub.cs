using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiscordSharp.Objects;

namespace DiscordSharp.Commands
{
    /// <summary>
    /// A basic command providing arguments as strings.
    /// </summary>
    public class CommandStub : ICommand
    {
        internal override Type __typeofCommand
        {
            get
            {
               return typeof(CommandStub);
            }
            set
            {
                base.__typeofCommand = value;
            }
        }

        internal CommandStub()
        {
            this.ID = IDGenerator.GenerateRandomCode();
        }

        public CommandStub(string name, string description, string helpTag)
        {
            this.ID = IDGenerator.GenerateRandomCode();

            CommandName = name;
            Description = description;
            HelpTag = helpTag;

            Args = new List<string>();
        }

        public CommandStub(string name, string description)
        {
            this.ID = IDGenerator.GenerateRandomCode();

            CommandName = name;
            Description = description;

            Args = new List<string>();
        }

        public CommandStub(Action<CommandArgs> action)
        {
            this.ID = IDGenerator.GenerateRandomCode();

            Do = action;

            Args = new List<string>();
        }

        public CommandStub(string name, string description, Action<CommandArgs> action)
        {
            this.ID = IDGenerator.GenerateRandomCode();

            Do = action;
            CommandName = name;
            Description = description;

            Args = new List<string>();
        }

        public CommandStub(string name, string description, string helpTag, Action<CommandArgs> action)
        {
            this.ID = IDGenerator.GenerateRandomCode();

            Do = action;
            CommandName = name;
            Description = description;
            HelpTag = helpTag;

            Args = new List<string>();
        }

        public CommandStub(string name, string description, string helpTag, PermissionType minPerm, Action<CommandArgs> action)
        {
            this.ID = IDGenerator.GenerateRandomCode();

            Do = action;
            CommandName = name;
            Description = description;
            HelpTag = helpTag;
            MinimumPermission = minPerm;

            Args = new List<string>();
        }

        public CommandStub(string name, string description, string helpTag, PermissionType minPerm, int argCount, Action<CommandArgs> action)
        {
            this.ID = IDGenerator.GenerateRandomCode();

            Do = action;
            CommandName = name;
            Description = description;
            HelpTag = helpTag;
            MinimumPermission = minPerm;
            ArgCount = argCount;

            Args = new List<string>();
        }

        [Obsolete]
        public override void ExecuteCommand()
        {
            CommandArgs e = new CommandArgs();
            e.Args = this.Args;
            Do.Invoke(e);
        }

        public override void ExecuteCommand(DiscordChannel channel, DiscordMember member)
        {
            CommandArgs e = new CommandArgs();
            e.Args = this.Args;
            e.Author = member;
            e.Channel = channel;

            if ((int)CommandsManager.GetPermissionFromID(member.ID) >= (int)MinimumPermission)
                Do.Invoke(e);
            else
                throw new UnauthorizedAccessException($"You have no permission to execute this command! (Minimum needed: {(MinimumPermission.ToString().Substring(MinimumPermission.ToString().IndexOf('.') + 1))})");
        }
    }
}
