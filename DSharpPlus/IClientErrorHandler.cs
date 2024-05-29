using System;
using System.Threading.Tasks;

namespace DSharpPlus;

/// <summary>
/// Represents a contract for handling errors the DSharpPlus core library may raise.
/// </summary>
public interface IClientErrorHandler
{
    /// <summary>
    /// Handles an error that occurred in an event handler.
    /// </summary>
    /// <param name="name">The name of the event.</param>
    /// <param name="exception">The exception thrown.</param>
    /// <param name="invokedDelegate">The delegate that was invoked.</param>
    /// <param name="sender">The object that dispatched this event.</param>
    /// <param name="args">The arguments passed to this event.</param>
    public ValueTask HandleEventHandlerError
    (
        string name,
        Exception exception,
        Delegate invokedDelegate,
        object sender,
        object args
    );

    /// <summary>
    /// Handles a gateway error of any kind.
    /// </summary>
    /// <param name="exception">The exception that occurred.</param>
    public ValueTask HandleGatewayError(Exception exception);
}
