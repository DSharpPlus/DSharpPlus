using System;
using System.Collections.Generic;

namespace DSharpPlus.SlashCommands;

/// <summary>
/// Specifies the installation context for a command or module.
/// </summary>
public class SlashCommandInstallTypeAttribute(params ApplicationIntegrationType[]  installTypes) : Attribute
{
    /// <summary>
    /// The contexts the command is allowed to be installed to.
    /// </summary>
    public IReadOnlyList<ApplicationIntegrationType> InstallTypes { get; } = installTypes;
}