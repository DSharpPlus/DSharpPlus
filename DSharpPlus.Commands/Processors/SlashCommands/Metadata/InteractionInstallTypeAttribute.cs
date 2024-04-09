using DSharpPlus.Entities;
using System;
using System.Collections.Generic;

namespace DSharpPlus.Commands.Processors.SlashCommands.Metadata;

/// <summary>
/// Specifies the installation context for a command or module.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class InteractionInstallTypeAttribute(params ApplicationIntegrationType[] installTypes) : Attribute
{
    /// <summary>
    /// The contexts the command is allowed to be installed to.
    /// </summary>
    public IReadOnlyList<ApplicationIntegrationType> InstallTypes { get; } = installTypes;
}
