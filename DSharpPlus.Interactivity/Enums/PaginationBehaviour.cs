namespace DSharpPlus.Interactivity.Enums;


/// <summary>
/// Specifies how pagination will handle advancing past the first and last pages.
/// </summary>
public enum PaginationBehaviour
{
    /// <summary>
    /// Going forward beyond the last page will loop back to the first page.
    /// Likewise, going back from the first page will loop around to the last page.
    /// </summary>
    WrapAround = 0,

    /// <summary>
    /// Attempting to go beyond the first or last page will be ignored.
    /// </summary>
    Ignore = 1
}
