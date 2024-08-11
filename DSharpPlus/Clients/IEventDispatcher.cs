using System.Threading.Tasks;

using DSharpPlus.EventArgs;

namespace DSharpPlus.Clients;

/// <summary>
/// Represents a service dispatching events to registered event handlers.
/// </summary>
public interface IEventDispatcher
{
    /// <summary>
    /// Dispatches the given event.
    /// </summary>
    /// <typeparam name="T">The type of event to dispatch.</typeparam>
    /// <param name="eventArgs">The event to dispatch.</param>
    /// <param name="client">The origin DiscordClient instance.</param>
    public ValueTask DispatchAsync<T>(DiscordClient client, T eventArgs)
        where T : DiscordEventArgs;
}
