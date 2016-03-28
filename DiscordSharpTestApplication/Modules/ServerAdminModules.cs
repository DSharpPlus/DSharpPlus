using DiscordSharp.Commands;
using DiscordSharp.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luigibot.Modules
{
    public class ServerAdminModules : IModule
    {
        private LuigibotMain mainEntry;
        public ServerAdminModules(LuigibotMain main)
        {
            mainEntry = main;
            Name = "server-admin-modules";
            Description = "Commands related to server owner exclusive commands like `gtfo`";
        }

        public override void Install(CommandsManager manager)
        {
            manager.AddCommand(new CommandStub("gtfo", "Makes the bot leave the server", "", PermissionType.User, cmdArgs =>
            {
                bool canExecute = false;
                foreach (var roll in cmdArgs.Author.Roles)
                    if (roll.Permissions.HasPermission(DiscordSpecialPermissions.ManageServer))
                        canExecute = true;
                if (cmdArgs.Author.Equals(mainEntry.owner))
                    canExecute = true;

                if (canExecute)
                {
                    if (cmdArgs.Channel.Parent.Owner.Equals(manager.Client.Me))
                    {
                        manager.Client.DeleteServer(cmdArgs.Channel.Parent);
                    }
                    else
                        manager.Client.LeaveServer(cmdArgs.Channel.Parent);
                }
                else
                    cmdArgs.Channel.SendMessage("You don't have the proper permissions to do this! You need the ManagerServer permission.");
            }), this);
        }
    }
}
