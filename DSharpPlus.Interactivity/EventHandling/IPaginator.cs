using System.Threading.Tasks;
namespace DSharpPlus.Interactivity.EventHandling;

internal interface IPaginator
{
    /// <summary>
    /// Paginates.
    /// </summary>
    /// <param name="request">The request to paginate.</param>
    /// <returns>A task that completes when the pagination finishes or times out.</returns>
    Task DoPaginationAsync(IPaginationRequest request);

    /// <summary>
    /// Disposes this EventWaiter
    /// </summary>
    void Dispose();
}
