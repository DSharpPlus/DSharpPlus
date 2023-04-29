using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace DSharpPlus.CH.Message.Internals;

internal class MessageMiddlewareHandler
{
    private readonly List<Type> _middlewares;
    private readonly Func<Task> _executeCommand;

    internal MessageMiddlewareHandler(List<Type> middlewares, Func<Task> executeCommand)
    {
        _middlewares = middlewares;
        _executeCommand = executeCommand;
    }

    internal async Task StartGoingThroughMiddlewaresAsync(MessageContext context, IServiceScope scope)
    {
         IMessageMiddleware[] middlewareQueue = new IMessageMiddleware[_middlewares.Count];

        for (int i = 0; i < _middlewares.Count; i++)
        {
            Type type = _middlewares[i];

            object?[]? constructorParameters = null;
            if (type.GetConstructors().Length != 0)
            {
                ConstructorInfo constructor = type.GetConstructors()[0];
                if (constructor.GetParameters().Length != 0)
                {
                    ParameterInfo[] parameters = constructor.GetParameters();
                    constructorParameters = new object?[parameters.Length];

                    for (int ii = 0; ii < parameters.Length; ii++)
                    {
                        constructorParameters[ii] = scope.ServiceProvider.GetService(parameters[ii].ParameterType);
                    }
                }
            }

            IMessageMiddleware middleware = (IMessageMiddleware)Activator.CreateInstance(type, constructorParameters)!;
            middlewareQueue[i] = middleware;
        }

        bool executeCommand = true;
        foreach (IMessageMiddleware middleware in middlewareQueue)
        {
            if (await middleware.InvokeAsync(context))
            {
                executeCommand = false;
            }
        }

        if (executeCommand)
        {
            await _executeCommand();
        }
    }
}

