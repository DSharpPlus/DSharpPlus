#if DEBUG
using System.Diagnostics;
#endif
using DSharpPlus.UnifiedCommands.Message.Conditions;
using DSharpPlus.UnifiedCommands.Internals.Trees;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DSharpPlus.UnifiedCommands.Message.Internals;

internal class MessageFactory
{
    private readonly TreeParent<MessageMethodData> _commands = new(string.Empty);
    private readonly List<Func<IServiceProvider, IMessageCondition>> _messageConditionBuilders = new();
    private readonly IServiceProvider _services;

    internal ITreeParent<MessageMethodData> GetTree() => _commands;

    public MessageFactory(IServiceProvider services)
        => _services = services;

    internal void AddMessageConditionBuilder(Func<IServiceProvider, IMessageCondition> func)
        => _messageConditionBuilders.Add(func);

    internal void ConstructAndExecuteCommand(Entities.DiscordMessage message,
        DiscordClient client, ReadOnlySpan<char> args, List<Range> argsRange)
    {
#if DEBUG
        long startTime = Stopwatch.GetTimestamp();
#endif

        Index end = 0;
        int it = 0;

        client.Logger.LogTrace("Found {Amount} of nodes at the first level.", _commands.List.Count);

        ITreeChild<MessageMethodData> treeChild;
        try
        {
            (ITreeChild<MessageMethodData>, int) result = _commands.Traverse(args[argsRange[0].Start..]);
            treeChild = result.Item1;
            it = result.Item2;
        }
        catch (KeyNotFoundException)
        {
            return;
        }

        if (treeChild.Value is null)
        {
            return;
        }

        string name = args[..end].ToString();

        int quoteStart = 0;
        bool doingQuoteString = false;
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
                    if (argSpan.StartsWith("\"") && argSpan.EndsWith("\""))
                    {
                        arguments.Add(new Range(argRange.Start.Value + 1, argRange.End.Value - 1));
                        continue;
                    }
                    else if (argSpan.StartsWith("\"") && !doingQuoteString)
                    {
                        quoteStart = argRange.Start.Value + 1;
                        doingQuoteString = true;
                        continue;
                    }
                    else if (doingQuoteString && argSpan.EndsWith("\""))
                    {
                        arguments.Add(new Range(quoteStart, argRange.End.Value - 1));
                        doingQuoteString = false;
                        continue;
                    }
                    else if (doingQuoteString)
                    {
                        continue;
                    }

                    arguments.Add(argRange);
                    continue;
                }

                if (argSpan.StartsWith("\"") && argSpan.EndsWith("\""))
                {
                    options.Add(args[lastOption!.Value.Start..lastOption.Value.End].ToString(),
                        new Range(argRange.Start.Value + 1, argRange.End.Value - 1));
                }
                else if (argSpan.StartsWith("\"") && !doingQuoteString)
                {
                    quoteStart = argRange.Start.Value + 1;
                    doingQuoteString = true;
                }
                else if (doingQuoteString && argSpan.EndsWith("\""))
                {
                    options.Add(args[lastOption!.Value.Start..lastOption.Value.End].ToString(),
                        new Range(quoteStart, argRange.End.Value - 1));
                    doingQuoteString = false;
                }
                else if (doingQuoteString)
                {
                }
                else
                {
                    options.Add(args[lastOption!.Value.Start..lastOption.Value.End].ToString(), argRange);
                }
            }
        }

        ArraySegment<char> segment = args.ToArray();

        // Loc = location
        int positionalArgumentLoc = 0;
        List<(Type, ArraySegment<char>)> mappedValues = new(arguments.Count + options.Count);
        IServiceScope scope = _services.CreateScope(); // This will need to be disposed later somehow.

        client.Logger.LogDebug("Parameters count is {Count}", treeChild.Value.Parameters.Count);
        client.Logger.LogDebug("Found {OptionsCount} options and {ArgumentCount} arguments", options.Count, arguments.Count);
        if (options.Count != 0)
        {
            client.Logger.LogDebug("First option has key {Key}", options.First().Key);
        }
        foreach (MessageParameterData data in treeChild.Value.Parameters)
        {
            client.Logger.LogDebug("Name is {Name}", data.Name);
            if (data.IsPositionalArgument)
            {
                if (positionalArgumentLoc > arguments.Count)
                {
                    // Throw this to the error handler
                    return;
                }

                Range range = arguments[positionalArgumentLoc];
                ArraySegment<char> argument = segment[range];
                mappedValues.Add((data.ConverterType, argument));
                positionalArgumentLoc++;
            }
            else if (options.TryGetValue(data.Name, out Range? optionRange))
            {
                ArraySegment<char> option;
                if (optionRange is Range r)
                {
                    option = segment[r];
                }
                else
                {
                    if (data.ShorthandOptionName is not null
                        && options.TryGetValue(data.ShorthandOptionName, out Range? shorthandOptionRange))
                    {
                        if (shorthandOptionRange is Range ra)
                        {
                            option = segment[ra];
                        }
                        else if (data.CanBeNull)
                        {
                            option = ArraySegment<char>.Empty;
                        }
                        else
                        {
                            return;
                        }
                    }
                    else if (data.CanBeNull)
                    {
                        option = ArraySegment<char>.Empty;
                    }
                    else
                    {
                        // Return to error handler.
                        return;
                    }
                }
                mappedValues.Add((data.ConverterType, option));
            }
            else
            {
                throw new Exception("Couldn't find anything for param.");
            }
        }
        client.Logger.LogDebug("Mapped values count is {Count}", mappedValues.Count);

#if DEBUG
        client.Logger.LogDebug("Took {NsExecution}ns", Stopwatch.GetElapsedTime(startTime).TotalNanoseconds);
#endif

        MessageHandler handler = new(message, treeChild.Value, scope, client, mappedValues, name,
            _messageConditionBuilders);
        _ = handler.BuildModuleAndExecuteCommandAsync();
    }
}
