using System;
using System.Threading.Tasks;

namespace DSharpPlus.AsyncEvents;

/// <summary>
/// Provides a registration surface for asynchronous events using C# language event syntax. 
/// </summary>
/// <typeparam name="TSender">The type of the event dispatcher.</typeparam>
/// <typeparam name="TArgs">The type of the argument object for this event.</typeparam>
/// <param name="sender">The instance that dispatched this event.</param>
/// <param name="args">The arguments passed to this event.</param>
public delegate Task AsyncEventHandler<in TSender, in TArgs>
(
    TSender sender,
    TArgs args
)
    where TArgs : AsyncEventArgs;

/// <summary>
/// Provides a registration surface for a handler for exceptions raised by an async event or its registered
/// event handlers.
/// </summary>
/// <typeparam name="TSender">The type of the event dispatcher.</typeparam>
/// <typeparam name="TArgs">The type of the argument object for this event.</typeparam>
/// <param name="event">The async event that threw this exception.</param>
/// <param name="exception">The thrown exception.</param>
/// <param name="handler">The async event handler that threw this exception.</param>
/// <param name="sender">The instance that dispatched this event.</param>
/// <param name="args">The arguments passed to this event.</param>
public delegate void AsyncEventExceptionHandler<TSender, TArgs>
(
    AsyncEvent<TSender, TArgs> @event,
    Exception exception,
    AsyncEventHandler<TSender, TArgs> handler,
    TSender sender,
    TArgs args
)
    where TArgs : AsyncEventArgs;
