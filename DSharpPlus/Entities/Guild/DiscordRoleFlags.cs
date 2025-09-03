using System;

namespace DSharpPlus.Entities;

/// <summary>
/// Flags describing a role's properties.
/// </summary>
[Flags]
public enum DiscordRoleFlags
{
    /// <summary>
    /// No flags are set.
    /// </summary>
    None = 0,
    
    /// <summary>
    ///	Role can be selected by members in an onboarding prompt.
    /// </summary>
    InPrompt = 1 << 0,
}
