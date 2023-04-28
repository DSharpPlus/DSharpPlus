using Microsoft.Extensions.DependencyInjection;

namespace DSharpPlus.CH.Message.Internals;

internal class MessageCommandFactory
{
    private readonly MessageCommandTree _commands = new();
    internal ServiceProvider _services = null!;
    internal CHConfiguration _configuration = null!;

    internal void AddCommand(string name, MessageCommandMethodData data) => _commands.Branches!.Add(name, new(data));
    internal void AddBranch(string name, MessageCommandTree branch) => _commands.Branches!.Add(name, branch);
    internal MessageCommandTree? GetBranch(string name) => _commands.Branches!.TryGetValue(name, out MessageCommandTree? result) ? result : null;

    internal async Task ConstructAndExecuteCommandAsync(Entities.DiscordMessage message,
        DiscordClient client, string[]? args)
    {
        string name = string.Empty;
        MessageCommandTree? tree = null;
        foreach (string arg in args ?? Array.Empty<string>())
        {
            if (tree is null)
            {
                if (_commands.Branches!.TryGetValue(arg, out MessageCommandTree? result))
                {
                    tree = result;
                    name += arg;
                }
                else
                {
                    break;
                }
                continue;
            }

            Console.WriteLine(arg);
            if (tree?.Branches is not null && tree.Branches.TryGetValue(arg, out MessageCommandTree? res))
            {
                tree = res;
                name += arg;
            }
            else
            {
                break;
            }
        }

        if (tree is null || tree.Data is null)
        {
            return;
        }

        IServiceScope? scope = _services.CreateScope();
        if (_configuration.Middlewares.Count != 0)
        {
            async Task PreparedFunction()
            {
                await ExecuteCommandAsync(tree.Data, message, client, scope, args);
            }

            MessageMiddlewareHandler? middlewareHandler = new(_configuration.Middlewares, PreparedFunction);
            await middlewareHandler.StartGoingThroughMiddlewaresAsync(new MessageContext
            {
                Message = message,
                Data = new MessageCommandData(name, tree.Data.Method) // I gotta change this to something better later...
            }, scope);
        }
    }

    internal static async Task ExecuteCommandAsync(MessageCommandMethodData data, Entities.DiscordMessage message,
        DiscordClient client, IServiceScope scope, string[]? args)
    {
        Dictionary<string, object> options = new();
        List<string> arguments = new();

        void Parse(Dictionary<string, object> options, List<string> arguments)
        {
            if (args is null)
            {
                return;
            }

            for (int i = 0; i < args.Length; i++)
            {
                ReadOnlySpan<char> str = args[i].AsSpan();

                if (str.StartsWith("-"))
                {
                    ReadOnlySpan<char> name = str.StartsWith("--") ? str[1..] : str[0..];

                }
            }
        }

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
                        arguments.Add(arg);
                    }
                    else if (tuple is null)
                    {
                        throw new NotImplementedException();
                    }
                    else
                    {
                        tuple = tuple.Item2 is null ? new Tuple<string, object?>(tuple.Item1, arg) : throw new NotImplementedException();
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
