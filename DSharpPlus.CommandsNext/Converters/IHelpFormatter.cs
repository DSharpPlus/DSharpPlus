using System.Collections.Generic;
using DSharpPlus.CommandsNext.Entities;

namespace DSharpPlus.CommandsNext.Converters
{
    /// <summary>
    /// Represents a minimum set of methods that a help formatter needs to implement.
    /// </summary>
    public interface IHelpFormatter
    {
        /// <summary>
        /// Sets the name of the current command.
        /// </summary>
        /// <param name="name">Name of the command for which the help is displayed.</param>
        /// <returns>Current formatter.</returns>
        IHelpFormatter WithCommandName(string name);

        /// <summary>
        /// Sets the description of the current command.
        /// </summary>
        /// <param name="description">Description of the command for which help is displayed.</param>
        /// <returns>Current formatter.</returns>
        IHelpFormatter WithDescription(string description);

        /// <summary>
        /// Sets the arguments the current command takes.
        /// </summary>
        /// <param name="arguments">Arguments that the command for which help is displayed takes.</param>
        /// <returns>Current formatter.</returns>
        IHelpFormatter WithArguments(IEnumerable<CommandArgument> arguments);

        /// <summary>
        /// Sets aliases for the current command.
        /// </summary>
        /// <param name="aliases">Aliases of the command for which help is displayed.</param>
        /// <returns>Current formatter.</returns>
        IHelpFormatter WithAliases(IEnumerable<string> aliases);

        /// <summary>
        /// Sets subcommands of the current command. This is also invoked for top-level command listing.
        /// </summary>
        /// <param name="subcommands">Subcommands of the command for which help is displayed.</param>
        /// <returns>Current formatter.</returns>
        IHelpFormatter WithSubcommands(IEnumerable<Command> subcommands);

        /// <summary>
        /// When the current command is a group, this sets it as executable.
        /// </summary>
        /// <returns>Current formatter.</returns>
        IHelpFormatter WithGroupExecutable();

        /// <summary>
        /// Construct the help message.
        /// </summary>
        /// <returns>Data for the help message.</returns>
        CommandHelpMessage Build();
    }
}
