using System;
using System.Threading;
using System.Threading.Tasks;

namespace DSharpPlus.Net.InboundWebhooks.Transport;

/// <summary>
/// Handles communication with the bot through HTTP interactions.
/// </summary>
public interface IInteractionTransportService
{
    /// <summary>
    /// Handles an interaction coming from the registered HTTP webhook.
    /// </summary>
    /// <param name="payload">The payload of the http request. This must be UTF-8 encoded.</param>
    /// <param name="token">A token to cancel the interaction when the http request was canceled</param>
    /// <returns>Returns the body which should be returned to the http request</returns>
    /// <exception cref="TaskCanceledException">Thrown when the passed cancellation token was canceled</exception>
    public Task<byte[]> HandleHttpInteractionAsync(ArraySegment<byte> payload, CancellationToken token);
}
