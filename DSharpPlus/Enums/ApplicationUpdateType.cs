namespace DSharpPlus;

/// <summary>
/// Defines the type of entity that was updated.
/// </summary>
public enum ApplicationUpdateType
{
    /// <summary>
    ///  A role was updated.
    /// </summary>
    Role = 1,
    /// <summary>
    /// A user was updated.
    /// </summary>
    User = 2,

    /// <summary>
    /// A channel was updated.
    /// </summary>
    Channel = 3
}
