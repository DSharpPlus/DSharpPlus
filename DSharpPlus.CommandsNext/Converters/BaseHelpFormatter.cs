using System.Collections.Generic;
using DSharpPlus.CommandsNext.Entities;

namespace DSharpPlus.CommandsNext.Converters
{
    /// <summary>
    /// Represents a base class for all default help formatters.
    /// </summary>
    public abstract class BaseHelpFormatter
    {
        /// <summary>
        /// Gets the CommandsNext extension which constructed this help formatter.
        /// </summary>
        protected CommandsNextExtension CommandsNext { get; }

        /// <summary>
        /// Creates a new help formatter for specified CommandsNext extension instance.
        /// </summary>
        /// <param name="cnext">CommandsNext instance this formatter is for</param>
        public BaseHelpFormatter(CommandsNextExtension cnext)
        {
            this.CommandsNext = cnext;
        }

        /// <summary>
        /// Sets the name of the current command.
        /// </summary>
        /// <param name="name">Name of the command for which the help is displayed.</param>
        /// <returns>Current formatter.</returns>
        public abstract BaseHelpFormatter WithCommandName(string name);

        /// <summary>
        /// Gets the full name of the current command, including all module names.
        /// </summary>
        /// <param name="name">Qualified name of the command for which the help is displayed.</param>
        /// <returns>Current formatter.</returns>
        public abstract BaseHelpFormatter WithQualifiedCommandName(string name);

        /// <summary>
        /// Sets the description of the current command.
        /// </summary>
        /// <param name="description">Description of the command for which help is displayed.</param>
        /// <returns>Current formatter.</returns>
        public abstract BaseHelpFormatter WithDescription(string description);

        /// <summary>
        /// Sets the arguments the current command takes.
        /// </summary>
        /// <param name="arguments">Arguments that the command for which help is displayed takes.</param>
        /// <returns>Current formatter.</returns>
        public abstract BaseHelpFormatter WithArguments(IEnumerable<CommandArgument> arguments);

        /// <summary>
        /// Sets aliases for the current command.
        /// </summary>
        /// <param name="aliases">Aliases of the command for which help is displayed.</param>
        /// <returns>Current formatter.</returns>
        public abstract BaseHelpFormatter WithAliases(IEnumerable<string> aliases);

        /// <summary>
        /// Sets subcommands of the current command. This is also invoked for top-level command listing.
        /// </summary>
        /// <param name="subcommands">Subcommands of the command for which help is displayed.</param>
        /// <returns>Current formatter.</returns>
        public abstract BaseHelpFormatter WithSubcommands(IEnumerable<Command> subcommands);

        /// <summary>
        /// This flags the current command as a group, with a bool stating whether or not the group is executable.
        /// </summary>
        /// <param name="groupExecutable">States whether or not the group is executable.</param>
        /// <returns>Current formatter.</returns>
        public abstract BaseHelpFormatter AsCommandGroup(bool groupExecutable);

        /// <summary>
        /// Construct the help message.
        /// </summary>
        /// <returns>Data for the help message.</returns>
        public abstract CommandHelpMessage Build();
    }
}
