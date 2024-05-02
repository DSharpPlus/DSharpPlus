namespace DSharpPlus.Entities;


/// <summary>
/// Represents a channel permissions overwrite's type.
/// </summary>
public enum DiscordOverwriteType : int
{
    /// <summary>
    /// Specifies that this overwrite applies to a role.
    /// </summary>
    Role,

    /// <summary>
    /// Specifies that this overwrite applies to a member.
    /// </summary>
    Member
}
