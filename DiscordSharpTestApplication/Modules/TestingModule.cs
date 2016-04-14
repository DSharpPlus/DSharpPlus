using DiscordSharp.Commands;
using DiscordSharp.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luigibot.Modules
{
    class TestingModule : IModule
    {
        public TestingModule()
        {
            Name = "testing";
            Description = "Testing modules for DiscordSharp.";
        }

        private static string RolesToString(List<DiscordRole> roles)
        {
            string returnValue = "{ ";
            for (int i = 0; i < roles.Count; i++)
            {
                returnValue += $"{roles[i].Name.Trim('@')} ({roles[i].ID})";
                if (i < roles.Count - 1)
                    returnValue += ", ";
            }
            return returnValue + " }";
        }

        public override void Install(CommandsManager manager)
        {
            manager.AddCommand(new CommandStub("rolesid", "Rolls.", "No mentions.", PermissionType.Owner, 1, cmdArgs =>
            {
                if(cmdArgs.Args.Count > 0)
                {
                    DiscordMember member = cmdArgs.Channel.Parent.Members.First(x => x.Value.ID == cmdArgs.Args[0]).Value;
                    if(member != null)
                    {
                        cmdArgs.Channel.SendMessage($"```\n{RolesToString(member.Roles)}\n```");
                    }
                }
            }), this);

            manager.AddCommand(new CommandStub("roles", "Rolls.", "No mentions.", PermissionType.Owner, 1, cmdArgs =>
            {
                if (cmdArgs.Args.Count > 0)
                {
                    DiscordMember member = cmdArgs.Channel.Parent.Members.First(x => x.Value.Username == cmdArgs.Args[0]).Value;
                    if (member != null)
                    {
                        cmdArgs.Channel.SendMessage($"```\n{RolesToString(member.Roles)}\n```");
                    }
                }
            }), this);

            manager.AddCommand(new CommandStub("userswithname", "yay", "No mentions.", PermissionType.Owner, 1, cmdArgs =>
            {
                if (cmdArgs.Args.Count > 0)
                {
                    string msg = "**Users with passed Username in this Server**\n\n";
                    foreach(var member in cmdArgs.Channel.Parent.Members)
                    {
                        if (member.Value.Username.ToLower() == cmdArgs.Args[0].ToLower())
                            msg += $"* {member.Value.Username} ({member.Value.ID})";
                    }
                    cmdArgs.Channel.SendMessage(msg);
                }
            }), this);

            manager.AddCommand(new CommandStub("rename", "Renames the bot.", "", PermissionType.Owner, 1, cmdArgs =>
            {
                if(cmdArgs.Args.Count > 0)
                {
                    var userInfo = manager.Client.ClientPrivateInformation;
                    userInfo.Username = cmdArgs.Args[0];

                    try
                    {
                        manager.Client.ChangeClientInformation(userInfo);
                    }
                    catch(Exception ex)
                    {
                        cmdArgs.Channel.SendMessage($"Error occured while renaming: {ex.Message}");
                    }
                }
            }), this);
        }
    }
}
