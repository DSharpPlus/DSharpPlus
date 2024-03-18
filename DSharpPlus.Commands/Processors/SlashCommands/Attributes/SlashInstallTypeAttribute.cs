namespace DSharpPlus.Commands.Processors.SlashCommands.Attributes;

using System;
using System.Collections.Generic;

/// <summary>
/// Specifies the installation context for a command or module.
/// </summary>
public class SlashInstallTypeAttribute(params ApplicationIntegrationType[]  installTypes) : Attribute
{
    public IReadOnlyList<ApplicationIntegrationType> InstallTypes { get; } = installTypes;
}
