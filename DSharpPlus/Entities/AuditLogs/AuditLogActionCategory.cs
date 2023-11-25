namespace DSharpPlus.Entities.AuditLogs;

/// <summary>
/// Indicates audit log action category.
/// </summary>
public enum AuditLogActionCategory
{
    /// <summary>
    /// Indicates that this action resulted in creation or addition of an object.
    /// </summary>
    Create,

    /// <summary>
    /// Indicates that this action resulted in update of an object.
    /// </summary>
    Update,

    /// <summary>
    /// Indicates that this action resulted in deletion or removal of an object.
    /// </summary>
    Delete,

    /// <summary>
    /// Indicates that this action resulted in something else than creation, addition, update, deleteion, or removal of an object.
    /// </summary>
    Other
}
