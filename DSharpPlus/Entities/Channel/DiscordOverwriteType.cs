namespace DSharpPlus.Entities;


/// <summary>
/// Represents a channel permissions overwrite's type.
/// </summary>
public enum DiscordOverwriteType : int
{
    /// <summary>
    /// The overwrite type is not currently defined.
    /// </summary>
    None = 0,

    /// <summary>
    /// Specifies that this overwrite applies to a role.
    /// </summary>
    Role = 1,

    /// <summary>
    /// Specifies that this overwrite applies to a member.
    /// </summary>
    Member = 2
}
