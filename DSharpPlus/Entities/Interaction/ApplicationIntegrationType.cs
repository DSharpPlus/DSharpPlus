namespace DSharpPlus.Entities;

/// <summary>
/// Represents the type of integration for an application.
/// </summary>
public enum ApplicationIntegrationType
{
    /// <summary>
    /// Represents that the integration can be installed for a guild.
    /// </summary>
    GuildInstall,
    
    /// <summary>
    /// Represents that the integration can be installed for a user.
    /// </summary>
    UserInstall,
}
