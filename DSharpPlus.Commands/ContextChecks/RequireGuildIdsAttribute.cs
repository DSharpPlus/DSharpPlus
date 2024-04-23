using System;
using System.Collections.Generic;

namespace DSharpPlus.Commands.ContextChecks;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Delegate)]
public class RequireGuildIdsAttribute : ContextCheckAttribute
{
    public IReadOnlyList<ulong> GuildIds { get; init; }

    public RequireGuildIdsAttribute(params ulong[] guildIds)
    {
        if (guildIds.Length == 0)
        {
            throw new ArgumentException("You must provide at least one guild ID.");
        }

        this.GuildIds = guildIds;
    }

}
