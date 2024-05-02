using System;
using System.Collections.Generic;
using DSharpPlus.Entities;

namespace DSharpPlus.Commands.Processors.SlashCommands.Metadata;

/// <summary>
/// Specifies the installation context for a command or module.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class InteractionInstallTypeAttribute(params DiscordApplicationIntegrationType[] installTypes) : Attribute
{
    /// <summary>
    /// The contexts the command is allowed to be installed to.
    /// </summary>
    public IReadOnlyList<DiscordApplicationIntegrationType> InstallTypes { get; } = installTypes;
}
