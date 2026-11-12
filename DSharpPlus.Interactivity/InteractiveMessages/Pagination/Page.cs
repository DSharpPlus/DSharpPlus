namespace DSharpPlus.Interactivity.InteractiveMessages.Pagination;

/// <summary>
/// Represents the content of a page in a paginated message.
/// </summary>
/// <param name="Name">An optional name for the page, shown in page selection.</param>
/// <param name="Content">The content of this page.</param>
/// <param name="Index">The index of this page in the pagination process.</param>
/// <param name="TotalPages">The total amount of pages being paginated for.</param>
public sealed record Page(string? Name, string Content, int Index, int TotalPages);
