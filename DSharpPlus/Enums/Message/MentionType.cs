namespace DSharpPlus;

/// <summary>
/// Type of mention being made
/// </summary>
public enum MentionType
{
    /// <summary>
    /// No mention (wtf?)
    /// </summary>
    None = 0,

    /// <summary>
    /// Mentioned Username
    /// </summary>
    Username = 1,

    /// <summary>
    /// Mentioned Nickname
    /// </summary>
    Nickname = 2,

    /// <summary>
    /// Mentioned Channel
    /// </summary>
    Channel = 4,

    /// <summary>
    /// Mentioned Role
    /// </summary>
    Role = 8
}
