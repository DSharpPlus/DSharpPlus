using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace DSharpPlus.CH.Message.Internals;

internal class MessageMiddlewareHandler
{
    private int _completions = 0;
    private List<Type> _middlewares;
    private Queue<IMessageMiddleware> _middlewareQueue = new();
    private Func<Task> _executeCommand;

    internal MessageMiddlewareHandler(List<Type> middlewares, Func<Task> executeCommand)
    {
        _middlewares = middlewares;
        _executeCommand = executeCommand;
    }

    internal async Task StartGoingThroughMiddlewaresAsync(MessageContext context, IServiceScope scope)
    {
        _middlewareQueue.EnsureCapacity(_middlewares.Count);

        foreach (Type? type in _middlewares)
        {
            object?[]? constructorParameters = null;
            if (type.GetConstructors().Length != 0)
            {
                ConstructorInfo? constructor = type.GetConstructors()[0];
                if (constructor.GetParameters().Length != 0)
                {
                    ParameterInfo[]? parameters = constructor.GetParameters();
                    constructorParameters = new object?[parameters.Length];

                    for (int i = 0; i < parameters.Length; i++)
                    {
                        if (parameters[i].ParameterType != typeof(NextDelegate))
                        {
                            constructorParameters[i] = scope.ServiceProvider.GetService(parameters[i].ParameterType);
                        }
                        else
                        {
                            constructorParameters[i] = (NextDelegate)HandleNextAsync;
                        }
                    }
                }
            }

            IMessageMiddleware? middleware = (IMessageMiddleware)Activator.CreateInstance(type, constructorParameters)!;
            _middlewareQueue.Enqueue(middleware);
        }

        await _middlewareQueue.Dequeue().InvokeAsync(context);
    }

    public async Task HandleNextAsync(MessageContext context)
    {
        ++_completions;
        if (_middlewareQueue.Count != 0 && _completions != _middlewares.Count)
        {
            await _middlewareQueue.Dequeue().InvokeAsync(context);
        }
        else if (_middlewareQueue.Count == 0 && _completions == _middlewares.Count)
        {
            await _executeCommand();
        }
    }
}
