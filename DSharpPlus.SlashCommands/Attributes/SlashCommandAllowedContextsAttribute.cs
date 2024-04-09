using System;
using System.Collections.Generic;

namespace DSharpPlus.SlashCommands;

/// <summary>
/// Specifies the allowed interaction contexts for a command.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class SlashCommandAllowedContextsAttribute(params InteractionContextType[] allowedContexts) : Attribute
{
    /// <summary>
    /// The contexts the command is allowed to be used in.
    /// </summary>
    public IReadOnlyList<InteractionContextType> AllowedContexts { get; } = allowedContexts;
}
