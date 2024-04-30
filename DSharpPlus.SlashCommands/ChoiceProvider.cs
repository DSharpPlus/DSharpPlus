namespace DSharpPlus.SlashCommands;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Entities;

/// <summary>
/// Implementation of <see cref="IChoiceProvider"/> with access to service collection.
/// </summary>
public abstract class ChoiceProvider : IChoiceProvider
{
    /// <summary>
    /// Sets the choices for the slash command.
    /// </summary>
    public abstract Task<IEnumerable<DiscordApplicationCommandOptionChoice>> Provider();

    /// <summary>
    /// Sets the service provider.
    /// </summary>
    public IServiceProvider Services { get; set; }

    /// <summary>
    /// The optional ID of the Guild the command got registered for.
    /// </summary>
    public ulong? GuildId { get; set; }
}
