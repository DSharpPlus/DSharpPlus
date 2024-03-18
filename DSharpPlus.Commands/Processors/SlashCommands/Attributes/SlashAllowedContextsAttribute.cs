namespace DSharpPlus.Commands.Processors.SlashCommands.Attributes;

using System;
using System.Collections.Generic;

/// <summary>
/// Specifies the allowed interaction contexts for a command.
/// </summary>
public sealed class SlashAllowedContextsAttribute(params InteractionContextType[] allowedContexts) : Attribute
{
    public IReadOnlyList<InteractionContextType> AllowedContexts { get; } = allowedContexts;
}
