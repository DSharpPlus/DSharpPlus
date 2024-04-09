namespace DSharpPlus.Entities;

/// <summary>
/// Represents a stage instance's privacy level.
/// </summary>
public enum PrivacyLevel
{
    /// <summary>
    /// Indicates that the stage instance is publicly visible.
    /// </summary>
    Public = 1,

    /// <summary>
    /// Indicates that the stage instance is only visible to guild members.
    /// </summary>
    GuildOnly
}
