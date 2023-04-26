using Microsoft.Extensions.DependencyInjection;

namespace DSharpPlus.CH.Message.Internals;

internal class MessageCommandFactory
{
    private Dictionary<string, MessageCommandMethodData> _commands = new();
    internal ServiceProvider _services = null!;
    internal CHConfiguration _configuration = null!;

    internal void AddCommand(string name, MessageCommandMethodData data) => _commands.Add(name, data);

    internal async Task ConstructAndExecuteCommandAsync(string name, Entities.DiscordMessage message,
        DiscordClient client, string[]? args)
    {
        if (_commands.TryGetValue(name, out MessageCommandMethodData? data))
        {
            IServiceScope? scope = _services.CreateScope();
            if (_configuration.Middlewares.Count != 0)
            {
                async Task PreparedFunction()
                {
                    await ExecuteCommandAsync(data, message, client, scope, args);
                }

                MessageMiddlewareHandler? middlewareHandler = new(_configuration.Middlewares, PreparedFunction);
                await middlewareHandler.StartGoingThroughMiddlewaresAsync(new MessageContext
                {
                    Message = message,
                    Data = new MessageCommandData(name, data.Method) // I gotta change this to something better later...
                }, scope);
            }
        }
    }

    internal async Task ExecuteCommandAsync(MessageCommandMethodData data, Entities.DiscordMessage message,
        DiscordClient client, IServiceScope scope, string[]? args)
    {
        Dictionary<string, object>? options = new();
        Queue<string>? arguments = new();
        if (args is not null)
        {
            bool parsingOptions = false;
            Tuple<string, object?>? tuple = null;
            foreach (string? arg in args)
            {
                if (arg.StartsWith("--"))
                {
                    parsingOptions = true;
                    if (tuple is null)
                    {
                        tuple = new Tuple<string, object?>(arg.Remove(0, 2), null);
                    }
                    else
                    {
                        options.Add(tuple.Item1, tuple.Item2 ?? true);
                        tuple = new Tuple<string, object?>(arg.Remove(0, 2), null);
                    }
                }
                else if (arg.StartsWith('-'))
                {
                    parsingOptions = true;
                    if (tuple is not null)
                    {
                        options.Add(tuple.Item1, tuple.Item2 ?? true);

                        tuple = new Tuple<string, object?>(arg.Remove(0, 1), null);
                    }
                    else
                    {
                        tuple = new Tuple<string, object?>(arg.Remove(0, 1), null);
                    }
                }
                else
                {
                    if (!parsingOptions)
                    {
                        arguments.Enqueue(arg);
                    }
                    else if (tuple is null)
                    {
                        throw new NotImplementedException();
                    }
                    else if (tuple.Item2 is null)
                    {
                        tuple = new Tuple<string, object?>(tuple.Item1, arg);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
            }

            if (tuple is not null)
            {
                options.Add(tuple.Item1, tuple.Item2 ?? true);
            }
        }

        MessageCommandHandler? handler = new();
        await handler.BuildModuleAndExecuteCommandAsync(data, scope, message, client, options, arguments);
    }
}
