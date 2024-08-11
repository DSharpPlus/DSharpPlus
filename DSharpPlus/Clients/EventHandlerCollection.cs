using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using DSharpPlus.EventArgs;

using Microsoft.Extensions.DependencyInjection;

using Handler = System.Func<DSharpPlus.DiscordClient, DSharpPlus.EventArgs.DiscordEventArgs, System.IServiceProvider, System.Threading.Tasks.Task>;
using SimpleHandler = System.Func<DSharpPlus.DiscordClient, DSharpPlus.EventArgs.DiscordEventArgs, System.Threading.Tasks.Task>;

namespace DSharpPlus;

/// <summary>
/// Contains an in-construction, mutable, list of event handlers filtered by event name.
/// </summary>
public sealed class EventHandlerCollection
{
    /// <summary>
    /// Gets the registered handlers for a type, or an empty collection if none were configured. These are returned as
    /// <see langword="object"/>, and individual elements must be casted to
    /// <c>Func&lt;DiscordClient, TEventArgs, IServiceProvider, Task&gt;</c>.
    /// </summary>
    public IReadOnlyList<object> this[Type type]
    {
        get
        {
            return this.Handlers.TryGetValue(type, out IReadOnlyList<object>? handlers)
                ? handlers
                : ([]);
        }
    }

    /// <summary>
    /// The event handlers configured for this application.
    /// </summary>
    private FrozenDictionary<Type, IReadOnlyList<object>> Handlers
        => this.immutableHandlers ?? AsImmutable();

    private FrozenDictionary<Type, IReadOnlyList<object>>? immutableHandlers = null;

    private readonly Dictionary<Type, List<object>> handlers = [];

    /// <summary>
    /// Registers a single simple event handler.
    /// </summary>
    /// <typeparam name="T">The type of event args this handler consumes.</typeparam>
    /// <param name="handler">The event handler.</param>
    public void Register<T>(Func<DiscordClient, T, Task> handler)
        where T : DiscordEventArgs
    {
        if (this.handlers.TryGetValue(typeof(T), out List<object>? value))
        {
            value.Add(CanonicalizeSimpleDelegateHandler(Unsafe.As<SimpleHandler>(handler)));
        }
        else
        {
            List<object> temporary = [];
            temporary.Add(CanonicalizeSimpleDelegateHandler(Unsafe.As<SimpleHandler>(handler)));

            this.handlers.Add(typeof(T), temporary);
        }
    }

    /// <summary>
    /// Registers all type-wise event handlers implemented by a specific type.
    /// </summary>
    /// <typeparam name="T">The type whose handlers to register.</typeparam>
    public void Register<T>()
        where T : IEventHandler
    {
        foreach ((Type args, Handler handler) in CanonicalizeTypeHandlerImplementations(typeof(T)))
        {
            if (this.handlers.TryGetValue(args, out List<object>? value))
            {
                value.Add(handler);
            }
            else
            {
                List<object> temporary = [handler];
                this.handlers.Add(args, temporary);
            }
        }
    }
    
    /// <summary>
    /// Produces a delegate matching the canonical representation from a simple, delegate-based handler.
    /// </summary>
    private static Handler CanonicalizeSimpleDelegateHandler(SimpleHandler simpleHandler) 
        => (client, args, _) => simpleHandler(client, args);

    /// <summary>
    /// Produces a delegate matching the canonical representation from an implementation of <see cref="IEventHandler{TEventArgs}"/>.
    /// </summary>
    private static IEnumerable<(Type, Handler)> CanonicalizeTypeHandlerImplementations(Type targetType)
    {
        return targetType.GetInterfaces()
            .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEventHandler<>))
            .Select(x => x.GetGenericArguments()[0])
            .Select(argumentType =>
            {
                ParameterExpression client = Expression.Parameter(typeof(DiscordClient), "client");
                ParameterExpression eventArgs = Expression.Parameter(argumentType, "eventArgs");
                ParameterExpression services = Expression.Parameter(typeof(IServiceProvider), "services");

                MethodInfo serviceProviderMethod = typeof(ServiceProviderServiceExtensions).GetMethod
                (
                    name: "GetRequiredService",
                    bindingAttr: BindingFlags.Public | BindingFlags.Static,
                    types: [typeof(IServiceProvider), typeof(Type)]
                )!;

                MethodInfo handlerMethod = targetType.GetMethod
                (
                    name: "HandleEventAsync",
                    bindingAttr: BindingFlags.Public | BindingFlags.Instance,
                    types: [typeof(DiscordClient), argumentType]
                )!;

                MethodCallExpression handlerCall = Expression.Call
                (
                    Expression.Convert
                    (
                        Expression.Call(null, serviceProviderMethod, services, Expression.Constant(targetType)),
                        targetType
                    ),
                    handlerMethod,
                    client, eventArgs
                );

                LambdaExpression lambda = Expression.Lambda(handlerCall, client, eventArgs, services);

                return (argumentType, Unsafe.As<Handler>(lambda.Compile()));
            });
    }

    private FrozenDictionary<Type, IReadOnlyList<object>> AsImmutable()
    {
        Dictionary<Type, IReadOnlyList<object>> copy = new(this.handlers.Count);

        foreach (KeyValuePair<Type, List<object>> handler in this.handlers)
        {
            copy.Add(handler.Key, handler.Value);
        }

        return this.immutableHandlers = copy.ToFrozenDictionary();
    }
}
