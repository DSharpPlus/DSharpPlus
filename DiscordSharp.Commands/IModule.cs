using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordSharp.Commands
{
    public abstract class IModule
    {
        /// <summary>
        /// The name of the module.
        /// </summary>
        public virtual string Name { get; set; } = "module";

        /// <summary>
        /// A description talking about what the module contains
        /// </summary>
        public virtual string Description { get; set; } = "Please set this in the constructor of your IModule derivative.";

        /// <summary>
        /// A list of the commands this module contains
        /// </summary>
        public virtual List<ICommand> Commands { get; internal set; } = new List<ICommand>();

        /// <summary>
        /// Installs the module's commands into the commands manager
        /// </summary>
        /// <param name="manager"></param>
        public abstract void Install(CommandsManager manager);

        /// <summary>
        /// Uninstall's this modules's commands from the given module manager.
        /// </summary>
        /// <param name="manager"></param>
        public void Uninstall(CommandsManager manager)
        {
            lock (manager.Commands)
            {
                foreach (var command in manager.Commands)
                {
                    var thisModulesCommand = Commands.Find(x => x.ID == command.ID && x.Parent.Name == this.Name); //compare modules by name just in case
                    if (thisModulesCommand != null)
                        manager.Commands.Remove(command);
                }
            }
        }

    }
}
