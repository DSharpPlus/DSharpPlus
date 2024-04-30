using System;
namespace DSharpPlus.SlashCommands;

/// <summary>
/// Indicates that a global application command cannot be invoked in DMs.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class GuildOnlyAttribute : Attribute { }
