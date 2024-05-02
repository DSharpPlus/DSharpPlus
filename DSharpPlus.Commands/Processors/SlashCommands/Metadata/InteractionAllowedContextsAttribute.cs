
using System;
using System.Collections.Generic;
using DSharpPlus.Entities;

namespace DSharpPlus.Commands.Processors.SlashCommands.Metadata;
/// <summary>
/// Specifies the allowed interaction contexts for a command.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class InteractionAllowedContextsAttribute(params DiscordInteractionContextType[] allowedContexts) : Attribute
{
    /// <summary>
    /// The contexts the command is allowed to be used in.
    /// </summary>
    public IReadOnlyList<DiscordInteractionContextType> AllowedContexts { get; } = allowedContexts;
}
