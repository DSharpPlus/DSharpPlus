using Microsoft.Extensions.DependencyInjection;

namespace DSharpPlus.CH.Message.Internals
{
    internal class MessageMiddlewareHandler
    {
        private int _completions = 0;
        private List<Type> _middlewares;
        private Queue<IMessageMiddleware> _middlewareQueue = new Queue<IMessageMiddleware>();
        private System.Func<Task> _executeCommand;

        internal MessageMiddlewareHandler(List<Type> middlewares, System.Func<Task> executeCommand)
        {
            _middlewares = middlewares;
            _executeCommand = executeCommand;
        }

        internal async Task StartGoingThroughMiddlewaresAsync(MessageContext context, IServiceScope scope)
        {
            _middlewareQueue.EnsureCapacity(_middlewares.Count);

            foreach (var type in _middlewares)
            {
                object?[]? constructorParameters = null;
                if (type.GetConstructors().Count() != 0)
                {
                    var constructor = type.GetConstructors()[0];
                    if (constructor.GetParameters().Count() != 0)
                    {
                        var parameters = constructor.GetParameters();
                        constructorParameters = new object?[parameters.Count()];

                        for (int i = 0; i < parameters.Count(); i++)
                        {
                            if (parameters[i].ParameterType != typeof(NextDelegate))
                                constructorParameters[i] = scope.ServiceProvider.GetService(parameters[i].ParameterType);
                            else
                                constructorParameters[i] = (NextDelegate)HandleNextAsync;
                        }
                    }
                }

                var middleware = (IMessageMiddleware)Activator.CreateInstance(type, constructorParameters)!;
                _middlewareQueue.Enqueue(middleware);
            }

            await _middlewareQueue.Dequeue().InvokeAsync(context);
        }

        public async Task HandleNextAsync(MessageContext context)
        {
            ++_completions;
            if (_middlewareQueue.Count != 0 && _completions != _middlewares.Count)
                await _middlewareQueue.Dequeue().InvokeAsync(context);
            else if (_middlewareQueue.Count == 0 && _completions == _middlewares.Count)
                await _executeCommand();
        }
    }
}