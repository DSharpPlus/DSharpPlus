using DiscordSharp.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordSharp.Commands
{
    public class CommandsManager
    {
        private readonly DiscordClient __client;
        internal DiscordClient Client
        {
            get
            {
                return __client;
            }
        }

        private List<ICommand> __commands;
        internal List<ICommand> Commands
        {
            get { return __commands; }
        }

        //id, permission 
        private static Dictionary<string, PermissionType> __internalUserRoles;

        internal static PermissionType GetPermissionFromID(string id)
        {
            if (__internalUserRoles.Count > 0)
                return __internalUserRoles.Select(m => m).Where(x => x.Key == id).First().Value;
            else
                return PermissionType.User;
        }

        public CommandsManager(DiscordClient client)
        {
            __client = client;
            __commands = new List<ICommand>();
            __internalUserRoles = new Dictionary<string, PermissionType>();
        }

        public void AddPermission(DiscordMember member, PermissionType permission) => __internalUserRoles.Add(member.ID, permission);
        public void AddPermission(string memberID, PermissionType permission) => __internalUserRoles.Add(memberID, permission);
        public void OverridePermissionsDictionary(Dictionary<string, PermissionType> dict) => __internalUserRoles = dict;

        public int ExecuteCommand(string rawCommandText, DiscordChannel channel, DiscordMember author)
        {
            string[] split = rawCommandText.Split(new char[] { ' ' }); //splits into args and stuff
            try
            {
                var command = __commands.Find(x => x.CommandName == split[0]);
                if(command != null)
                {
                    //adds all the arguments
                    for (int i = 1; i < split.Length; i++)
                        command.AddArgument(split[i]);
                    //finally, executes it
                    command.ExecuteCommand(channel, author);
                    return 0;
                }
            }
            catch(UnauthorizedAccessException uaex)
            {
                throw uaex; //no permission
            }
            catch(Exception ex)
            {
                throw ex;
            }
            return 1;
        }

        public void AddCommand(ICommand command) => __commands.Add(command);
    }
}
