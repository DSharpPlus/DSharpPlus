namespace DSharpPlus.SlashCommands;

using System;

/// <summary>
/// Indicates that a global application command cannot be invoked in DMs.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class GuildOnlyAttribute : Attribute { }
