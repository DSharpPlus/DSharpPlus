using DiscordSharp.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordSharp.Commands
{
    public class ModuleNotEnabledException : Exception
    {
        IModule module;
        public IModule Module
        {
            get { return module; }
        }
        public ModuleNotEnabledException(string message, IModule module) : base(message)
        {
            this.module = module;
        }
    }

    public class BaseModuleToggleException : Exception
    {
        public BaseModuleToggleException(string message) : base(message) { }
    }

    public class CommandsManager
    {
        private readonly DiscordClient __client;
        public DiscordClient Client
        {
            get
            {
                return __client;
            }
        }

        public Random rng = new Random((int)DateTime.Now.Ticks);

        private List<ICommand> __commands;
        public List<ICommand> Commands
        {
            get { return __commands; }
        }

        /// <summary>
        /// Key value pair of the modules.
        /// Key = module
        /// Value = Whether or not the module is enabled.
        /// </summary>
        private Dictionary<IModule, bool> __modules;
        public Dictionary<IModule, bool> Modules
        {
            get { return __modules; }
        }

        //id, permission 
        private static Dictionary<string, PermissionType> __internalUserRoles;
        public static Dictionary<string, PermissionType> UserRoles
        {
            get
            {
                return __internalUserRoles;
            }
        }

        internal static PermissionType GetPermissionFromID(string id)
        {
            if (__internalUserRoles.Count > 0)
            {
                foreach(var perm in __internalUserRoles)
                {
                    if (perm.Key == id)
                        return perm.Value;
                }
                return PermissionType.User;
            }
            else
                return PermissionType.User;
        }

        public CommandsManager(DiscordClient client)
        {
            __client = client;
            __commands = new List<ICommand>();
            __modules = new Dictionary<IModule, bool>();
            __internalUserRoles = new Dictionary<string, PermissionType>();
            Console.Write("");
        }

        public bool HasPermission(DiscordMember member, PermissionType permission)
        {
            if(__internalUserRoles.ContainsKey(member.ID))
            {
                foreach (var perm in __internalUserRoles)
                    if (perm.Key == member.ID && (int)perm.Value >= (int)permission)
                        return true;
            }
            return false;
        }

        public void AddPermission(DiscordMember member, PermissionType permission)
        {
            if (__internalUserRoles.ContainsKey(member.ID))
                __internalUserRoles.Remove(member.ID);
            __internalUserRoles.Add(member.ID, permission);
        }
        public void AddPermission(string memberID, PermissionType permission)
        {
            if (__internalUserRoles.ContainsKey(memberID))
                __internalUserRoles.Remove(memberID);
            __internalUserRoles.Add(memberID, permission);
        }

        public void OverridePermissionsDictionary(Dictionary<string, PermissionType> dict) => __internalUserRoles = dict;

        public void OverrideModulesDictionary(Dictionary<string, bool> dictionary)
        {
            foreach (IModule kvp in __modules.Keys.ToList())
            {
                if (kvp.Name.ToLower().Trim() != "base")
                {
                    if (dictionary.ContainsKey(kvp.Name.ToLower().Trim()))
                        __modules[kvp] = dictionary[kvp.Name.ToLower().Trim()];
                }
            }
        }

        public Dictionary<string, bool> ModuleDictionaryForJson()
        {
            Dictionary<string, bool> dict = new Dictionary<string, bool>();

            lock(__modules)
            {
                foreach (var kvp in __modules)
                {
                    if (kvp.Key.Name.ToLower().Trim() != "base")
                        dict.Add(kvp.Key.Name, kvp.Value);
                }
            }

            return dict;
        }

        public int ExecuteOnMessageCommand(string rawCommandText, DiscordChannel channel, DiscordMember author)
        {
            string[] split = rawCommandText.Split(new char[] { ' ' }); //splits into args and stuff
            try
            {
                var command = __commands.Find(x => x.CommandName == split[0]);

                if (command != null && command.Parent != null) //if it's a generic command without a parent then don't bother doing this.
                {
                    lock(__modules)
                    {
                        if (__modules[command.Parent] == false)
                        {
                            throw new ModuleNotEnabledException($"The specified module {command.Parent.Name} is not enabled.", command.Parent);
                        }
                    }
                }

                if(command != null)
                {
                    command.Args.Clear();
                    if (command.ArgCount > 0)
                    {
                        string[] argsSplit = rawCommandText.Split(new char[] { ' ' }, command.ArgCount + 1);
                        //adds all the arguments
                        for (int i = 1; i < argsSplit.Length; i++)
                            command.AddArgument(argsSplit[i]);
                    }
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

        public bool ModuleEnabled(string name)
        {
            lock(__modules)
            {
                foreach (var kvp in __modules)
                {
                    if (kvp.Key.Name.ToLower().Trim() == name.ToLower().Trim())
                    {
                        return __modules[kvp.Key];
                    }
                }
            }
            return false;
        }

        public void EnableModule(string name)
        {
            lock(__modules)
            {
                foreach (var kvp in __modules)
                {
                    if (kvp.Key.Name.ToLower().Trim() == name.ToLower().Trim()) //if module exists
                    {
                        __modules[kvp.Key] = true; //enabled
                        break;
                    }
                }
            }
        }

        public void DisableModule(string name)
        {
            if (name.ToLower().Trim() == "base")
                throw new BaseModuleToggleException("Can't disable base module!");

            lock(__modules)
            {
                foreach (var kvp in __modules)
                {
                    if (kvp.Key.Name.ToLower().Trim() == name.ToLower().Trim()) //if module exists
                    {
                        __modules[kvp.Key] = false; //disable it
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Adds a generic command without an associated module.
        /// </summary>
        /// <param name="command"></param>
        public void AddCommand(ICommand command) => __commands.Add(command);

        /// <summary>
        /// Adds a command with an assosciated module.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="fromModule"></param>
        public void AddCommand(ICommand command, IModule fromModule)
        {
            command.Parent = fromModule;
            command.Parent.Commands.Add(command);
            lock(__modules)
            {
                if (!__modules.ContainsKey(fromModule))
                    __modules.Add(fromModule, true);

                if (__modules[fromModule] == false) //if you're adding the command, you're enabling the module.
                    __modules[fromModule] = true;
            }

            __commands.Add(command);
        }
    }
}
