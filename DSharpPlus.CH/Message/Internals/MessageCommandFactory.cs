using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DSharpPlus.CH.Message.Internals;

internal class MessageCommandFactory
{
    private readonly MessageCommandTree _commands = new();
    internal ServiceProvider _services = null!;
    internal CHConfiguration _configuration = null!;

    internal void AddCommand(string name, MessageCommandMethodData data) => _commands.Branches!.Add(name, new(data));
    internal void AddBranch(string name, MessageCommandTree branch) => _commands.Branches!.Add(name, branch);

    internal MessageCommandTree? GetBranch(string name) =>
        _commands.Branches!.TryGetValue(name, out MessageCommandTree? result) ? result : null;

    internal void ConstructAndExecuteCommand(Entities.DiscordMessage message,
        DiscordClient client, ref ReadOnlySpan<char> args, List<Range> argsRange)
    {
        long startTime = Stopwatch.GetTimestamp();
        
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

        Dictionary<string, Range?> options = new();
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
                    lastOption = argSpan.StartsWith("--")
                        ? new(argRange.Start.Value + 2, argRange.End)
                        : new(argRange.Start.Value + 1, argRange.End);
                }
                else
                {
                    options.Add(argSpan.ToString(), null);
                    lastOption = argSpan.StartsWith("--")
                        ? new(argRange.Start.Value + 2, argRange.End)
                        : new(argRange.Start.Value + 1, argRange.End);
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

                options.Add(args[lastOption!.Value.Start..lastOption.Value!.End].ToString(), argRange);
            }
        }

        int positionalArgumentPosition = 0;
        Dictionary<string, string> mappedValues = new();
        foreach (MessageCommandParameterData data in tree.Data.Parameters)
        {
            if (options.TryGetValue(data.Name, out Range? value))
            {
                if (data.Type == MessageCommandParameterDataType.Bool && value is not null)
                {
                    // Error handling needs to be implemented.
                }
                else if (data.Type != MessageCommandParameterDataType.Bool && value is null)
                {
                }
                else if (data.Type == MessageCommandParameterDataType.User
                         || data.Type == MessageCommandParameterDataType.Channel
                         || data.Type == MessageCommandParameterDataType.Member)
                {
                    ReadOnlySpan<char> span = args[value!.Value.Start..value.Value.End];
                    if (!span.StartsWith("<@") || !span.StartsWith("<#"))
                    {
                        // Error handling;
                        continue;
                    }

                    ReadOnlySpan<char> formattedSpan = span is [_, _, '!', ..] ? span[2..^1] : span[1..^1];
                    mappedValues.Add(data.Name, formattedSpan.ToString());
                }
                else if (data.Type == MessageCommandParameterDataType.Role)
                {
                    ReadOnlySpan<char> span = args[value!.Value.Start..value.Value.End];
                    ReadOnlySpan<char> formattedSpan = span[2..^1];
                    mappedValues.Add(data.Name, formattedSpan.ToString());
                }
                else if (data.Type != MessageCommandParameterDataType.Bool)
                {
                    ReadOnlySpan<char> span = args[value!.Value.Start..value.Value.End];
                    mappedValues.Add(data.Name, span.ToString());
                }
                else
                {
                    mappedValues.Add(data.Name, "true");
                }
            }
            else
            {
                if (data.IsPositionalArgument)
                {
                    Range range = arguments[positionalArgumentPosition];
                    positionalArgumentPosition++;
                    ReadOnlySpan<char> span = args[range.Start..range.End];

                    if (data.Type == MessageCommandParameterDataType.User
                        || data.Type == MessageCommandParameterDataType.Channel
                        || data.Type == MessageCommandParameterDataType.Member)
                    {
                        if (!span.StartsWith("<@") || !span.StartsWith("<#"))
                        {
                            // Error handling;
                            continue;
                        }

                        ReadOnlySpan<char> formattedSpan = span is [_, _, '!', ..] ? span[2..^1] : span[1..^1];
                        mappedValues.Add(data.Name, formattedSpan.ToString());
                    }
                    else if (data.Type == MessageCommandParameterDataType.Role)
                    {
                        ReadOnlySpan<char> formattedSpan = span[2..^1];
                        mappedValues.Add(data.Name, formattedSpan.ToString());
                    }
                    else if (data.Type != MessageCommandParameterDataType.Bool)
                    {
                        mappedValues.Add(data.Name, span.ToString());
                    }
                    else
                    {
                        mappedValues.Add(data.Name, "true");
                    }
                }

                if (data.Type != MessageCommandParameterDataType.Bool)
                {
                    // Error handling.
                }
                else
                {
                    mappedValues.Add(data.Name, "false");
                }
            }
        }
        
        Console.WriteLine($"Took {Stopwatch.GetElapsedTime(startTime).TotalNanoseconds}ns");

        IServiceScope scope = _services.CreateScope(); // This will need to be disposed later somehow.

        string name = args[0..end].ToString();
        MessageCommandHandler handler = new(message, tree.Data, scope, client, mappedValues, _configuration, name);
        Task.Run(handler.BuildModuleAndExecuteCommandAsync);
    }
}
