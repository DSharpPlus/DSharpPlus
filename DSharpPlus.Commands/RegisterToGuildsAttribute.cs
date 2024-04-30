namespace DSharpPlus.Commands;

using System;
using System.Collections.Generic;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Delegate)]
public class RegisterToGuildsAttribute : Attribute
{
    /// <summary>
    /// The guild ids to register this command to.
    /// </summary>
    public IReadOnlyList<ulong> GuildIds { get; init; }

    /// <summary>
    /// Creates a new instance of the <see cref="RegisterToGuildsAttribute"/> class.
    /// </summary>
    /// <param name="guildIds">The guild ids to register this command to.</param>
    public RegisterToGuildsAttribute(params ulong[] guildIds)
    {
        if (guildIds.Length == 0)
        {
            throw new ArgumentException("You must provide at least one guild ID.");
        }

        GuildIds = guildIds;
    }
}
