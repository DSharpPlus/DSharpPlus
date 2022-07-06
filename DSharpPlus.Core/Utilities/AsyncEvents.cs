using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace DSharpPlus.Core.Utilities
{
    /// <summary>
    /// Registers an event handler for an async event.
    /// </summary>
    /// <param name="sender">The type thats expected to be executing the event.</param>
    /// <param name="args">The argument that the executor provides.</param>
    /// <returns>A task.</returns>
    public delegate Task AsyncEventHandler<in TSender, in TArgs>(TSender sender, TArgs args, CancellationToken cancellationToken);

    /// <summary>
    /// Handles any exceptions thrown by <see cref="AsyncEventHandler{TSender, TArgs}"/>.
    /// </summary>
    /// <param name="sender">The sender from <see cref="AsyncEventHandler{TSender, TArgs}"/>.</param>
    /// <param name="exception">The exception thrown by <see cref="AsyncEventHandler{TSender, TArgs}"/>.</param>
    /// <returns>A task.</returns>
    public delegate Task AsyncExceptionHandler<in TSender, in TArgs>(TSender sender, TArgs args, Exception exception, CancellationToken cancellationToken);

    /// <summary>
    /// Registers an event handler for an async event.
    /// </summary>
    /// <typeparam name="TSender">What the async event is being executed by.</typeparam>
    /// <typeparam name="TArgs"></typeparam>
    public sealed class AsyncEvent<TSender, TArgs>
    {
        private IEnumerable<AsyncEventHandler<TSender, TArgs>> PrivateHandlers = Array.Empty<AsyncEventHandler<TSender, TArgs>>();
        public ImmutableHashSet<AsyncEventHandler<TSender, TArgs>> Handlers => PrivateHandlers.ToImmutableHashSet();
        public AsyncExceptionHandler<TSender, TArgs> ExceptionHandler { get; init; }

        /// <summary>
        /// Constructs a new <see cref="AsyncEvent{TSender, TArgs}"/> for async and parallel execution.
        /// </summary>
        public AsyncEvent(AsyncExceptionHandler<TSender, TArgs> exceptionHandler) => ExceptionHandler = exceptionHandler;

        /// <summary>
        /// Adds an event handler to the async event.
        /// </summary>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Register(AsyncEventHandler<TSender, TArgs> handler)
        {
            IEnumerable<AsyncEventHandler<TSender, TArgs>> oldHandlers, newHandlers;
            do
            {
                oldHandlers = PrivateHandlers;
                newHandlers = oldHandlers.Append(handler);
            } while (Interlocked.CompareExchange(ref PrivateHandlers, newHandlers, oldHandlers) != oldHandlers);
        }

        /// <summary>
        /// Removes an event handler from the async event.
        /// </summary>
        /// <returns><see langword="true"/> if item is successfully removed; otherwise, <see langword="false"/>. This method also returns <see langword="false"/> if item was not found in the handler list.</returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Unregister(AsyncEventHandler<TSender, TArgs> handler)
        {
            IEnumerable<AsyncEventHandler<TSender, TArgs>> oldHandlers, newHandlers;
            do
            {
                oldHandlers = PrivateHandlers;
                newHandlers = oldHandlers.Except(new[] { handler });
            } while (Interlocked.CompareExchange(ref PrivateHandlers, newHandlers, oldHandlers) != oldHandlers);
        }

        /// <summary>
        /// Executes all handlers asyncronously.
        /// </summary>
        /// <param name="sender">The type thats expected to be executing the event.</param>
        /// <param name="args">The argument that the executor provides.</param>
        /// <returns>A task representing the execution of all event handlers.</returns>
        public Task InvokeAsync(TSender sender, TArgs args, CancellationToken cancellationToken = default) => Parallel.ForEachAsync(Handlers, cancellationToken, async (handler, cancellationToken) =>
        {
            try
            {
                await handler(sender, args, cancellationToken);
            }
            catch (Exception error)
            {
                if (ExceptionHandler != null)
                {
                    await ExceptionHandler(sender, args, error, cancellationToken);
                }
            }
        });
    }
}
