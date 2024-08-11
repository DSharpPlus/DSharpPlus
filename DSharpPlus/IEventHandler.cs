using System.ComponentModel;
using System.Threading.Tasks;

using DSharpPlus.EventArgs;

namespace DSharpPlus;

/// <summary>
/// Don't touch this.
/// </summary>
// an internal marker interface for event handlers.
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IEventHandler;

/// <summary>
/// Represents a base interface for an event handler.
/// </summary>
/// <typeparam name="TEventArgs">The type of event this handler is supposed to handle.</typeparam>
public interface IEventHandler<TEventArgs> : IEventHandler
    where TEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Handles the provided event asynchronously.
    /// </summary>
    /// <param name="sender">The DiscordClient this event originates from.</param>
    /// <param name="eventArgs">Any additional information pertaining to this event.</param>
    public Task HandleEventAsync(DiscordClient sender, TEventArgs eventArgs);
}
