using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace DSharpPlus.Interactivity.EventHandling;

public interface IPaginationRequest
{
    /// <summary>
    /// Returns the number of pages.
    /// </summary>
    /// <returns></returns>
    int PageCount { get; }

    /// <summary>
    /// Returns the current page.
    /// </summary>
    /// <returns></returns>
    Task<Page> GetPageAsync();

    /// <summary>
    /// Tells the request to set its index to the first page.
    /// </summary>
    /// <returns></returns>
    Task SkipLeftAsync();

    /// <summary>
    /// Tells the request to set its index to the last page.
    /// </summary>
    /// <returns></returns>
    Task SkipRightAsync();

    /// <summary>
    /// Tells the request to increase its index by one.
    /// </summary>
    /// <returns></returns>
    Task NextPageAsync();

    /// <summary>
    /// Tells the request to decrease its index by one.
    /// </summary>
    /// <returns></returns>
    Task PreviousPageAsync();

    /// <summary>
    /// Requests message emojis from pagination request.
    /// </summary>
    /// <returns></returns>
    Task<PaginationEmojis> GetEmojisAsync();

    /// <summary>
    /// Requests the message buttons from the pagination request.
    /// </summary>
    /// <returns>The buttons.</returns>
    Task<IEnumerable<DiscordButtonComponent>> GetButtonsAsync();

    /// <summary>
    /// Gets pagination message from this request.
    /// </summary>
    /// <returns></returns>
    Task<DiscordMessage> GetMessageAsync();

    /// <summary>
    /// Gets the user this pagination applies to.
    /// </summary>
    /// <returns></returns>
    Task<DiscordUser> GetUserAsync();

    /// <summary>
    /// Get this request's Task Completion Source.
    /// </summary>
    /// <returns></returns>
    Task<TaskCompletionSource<bool>> GetTaskCompletionSourceAsync();

    /// <summary>
    /// Tells the request to perform cleanup.
    /// </summary>
    /// <returns></returns>
    Task DoCleanupAsync();
}
