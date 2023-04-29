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

    internal void ConstructAndExecuteCommand(Entities.DiscordMessage message,
        DiscordClient client, ref ReadOnlySpan<char> args, List<Range> argsRange)
    {
        Index end = 0;
        int it = 0;

        MessageCommandTree? tree = null;

        foreach (Range argRange in argsRange)
        {
            ReadOnlySpan<char> arg = args[argRange.Start..argRange.End];

            if (tree is null)
            {
                if (_commands.Branches!.TryGetValue(arg.ToString(), out MessageCommandTree? res))
                {
                    tree = res;
                    end = argRange.End;
                    it++;
                }
                else
                {
                    break;
                }
                continue;
            }

            if (tree?.Branches is not null && tree.Branches.TryGetValue(arg.ToString(), out MessageCommandTree? res2))
            {
                tree = res2;
                end = argRange.End;
                it++;
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

        System.Diagnostics.Stopwatch sw = new();
        sw.Start();

        List<(Range, Range?)> options = new();
        List<Range> arguments = new();
        bool parsingArguments = true;
        Range? lastOption = null;
        for (int i = it; i < argsRange.Count; i++)
        {
            Range argRange = argsRange[i];
            ReadOnlySpan<char> argSpan = args[argRange.Start..argRange.End];

            if (argSpan.StartsWith("-"))
            {
                parsingArguments = false;
                if (lastOption is null)
                {
                    lastOption = argSpan.StartsWith("--") ? new(argRange.Start.Value + 2, argRange.End) :
                        new(argRange.Start.Value + 1, argRange.End);
                }
                else
                {
                    options.Add(((Range)lastOption, null));
                    lastOption = argSpan.StartsWith("--") ? new(argRange.Start.Value + 2, argRange.End) :
                        new(argRange.Start.Value + 1, argRange.End);
                }
            }
            else
            {
                if (lastOption is null && !parsingArguments)
                {
                    Console.WriteLine("Uh oh, floating value."); // Floating as it isn't related to anything whatsoever.
                    continue;
                }
                else if (parsingArguments)
                {
                    arguments.Add(argRange);
                    continue;
                }

                options.Add(((Range)lastOption!, argRange));
            }
        }

        sw.Stop();
        Console.WriteLine($"Took {sw.Elapsed.TotalNanoseconds}ns to process arguments");

        IServiceScope? scope = _services.CreateScope();
        if (_configuration.Middlewares.Count != 0)
        {
            async Task PreparedFunction()
            {
                await ExecuteCommandAsync(tree.Data, message, client, scope, testArgs.ToArray());
            }

            MessageMiddlewareHandler middlewareHandler = new(_configuration.Middlewares, PreparedFunction);
            string name = args[0..end].ToString();
            Task.Run(async () => await middlewareHandler.StartGoingThroughMiddlewaresAsync(new MessageContext
            {
                Message = message,
                Data = new MessageCommandData(name, tree.Data.Method) // I gotta change this to something better later...
            }, scope));
        }
    }

    internal static async Task ExecuteCommandAsync(MessageCommandMethodData data, Entities.DiscordMessage message,
        DiscordClient client, IServiceScope scope, string[]? args)
    {
        Console.WriteLine(args?.Length ?? null);
        Dictionary<string, object> options = new();
        List<string> arguments = new();

        if (args is not null)
        {
            bool parsingOptions = false;
            Tuple<string, object?>? tuple = null;
            foreach (string arg in args)
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
                Console.WriteLine(arg);
            }

            if (tuple is not null)
            {
                options.Add(tuple.Item1, tuple.Item2 ?? true);
            }
        }

        MessageCommandHandler handler = new();
        await handler.BuildModuleAndExecuteCommandAsync(data, scope, message, client, options, arguments);
    }
}
