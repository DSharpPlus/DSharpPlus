namespace DSharpPlus.SlashCommands;

using System.Collections.Generic;
using System.Threading.Tasks;

using DSharpPlus.Entities;

/// <summary>
/// All choice providers must inherit from this interface.
/// </summary>
public interface IChoiceProvider
{
    /// <summary>
    /// Sets the choices for the slash command.
    /// </summary>
    Task<IEnumerable<DiscordApplicationCommandOptionChoice>> Provider();
}
