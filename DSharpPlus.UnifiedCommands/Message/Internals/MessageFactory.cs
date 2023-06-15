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
    private readonly TreeParent<MessageMethodData> _commands = new();
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
            client.Logger.LogTrace("Hello there!");
            return;
        }

        List<ITreeChild<MessageMethodData>> overloads = new();
        if (treeChild.Value is null)
        {
            client.Logger.LogTrace("Key is {Key}", treeChild.Key);
            if (treeChild is ITreeParent<MessageMethodData> parent && parent.List.Count != 0)
            {
                foreach (ITreeChild<MessageMethodData> child in parent.List)
                {
                    if (child.Key is null && child.Value is not null)
                    {
                        overloads.Add(child);
                    }
                }
                if (overloads.Count == 0)
                {
                    return;
                }
            }
            else
            {
                return;
            }
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
        List<(Type, ArraySegment<char>?)> mappedValues = new(arguments.Count + options.Count);
        IServiceScope scope = _services.CreateScope(); // This will need to be disposed later somehow.

        MessageMethodData? methodData = null;
        if (overloads.Count == 0)
        {
            methodData = treeChild.Value!;
        }
        else
        {
            foreach (ITreeChild<MessageMethodData> child in overloads)
            {
                if (child.Value is null)
                {
                    continue;
                }

                int positionalArgumentCount = 0;
                foreach (MessageParameterData param in child.Value.Parameters)
                {
                    if (param.IsPositionalArgument)
                    {
                        positionalArgumentCount++;
                        continue;
                    }

                    if (!options.ContainsKey(param.Name))
                    {
                        if (param.CanBeNull)
                        {
                            continue;
                        }
                        goto label;
                    }
                }
                if (positionalArgumentCount != arguments.Count)
                {
                    continue;
                }
                methodData = child.Value;
label:;
            }

            if (methodData is null)
            {
                return;
            }
        }

        // Value should never be null here (hopefully)
        foreach (MessageParameterData data in methodData.Parameters)
        {
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
                if (optionRange is Range r)
                {
                    ArraySegment<char> option = segment[r];
                    mappedValues.Add((data.ConverterType, option));
                }
                else
                {
                    // Error handling
                }
            }
            else if (data.ShorthandOptionName is not null
                && options.TryGetValue(
                    data.ShorthandOptionName, out Range? shorthandOptionRange
                ))
            {
                if (shorthandOptionRange is Range r)
                {
                    ArraySegment<char> option = segment[r];
                    mappedValues.Add((data.ConverterType, option));
                }
                else
                {
                    // Error handling 
                }
            }
            else
            {
                if (!data.CanBeNull)
                {
                    throw new Exception($"Couldn't find anything for param {data.Name}.");
                }
                else
                {
                    mappedValues.Add((data.ConverterType, null));
                }
            }
        }
        client.Logger.LogDebug("Mapped values has a total amount of {Count}", mappedValues.Count);

#if DEBUG // Cry about it, Idk how else to disble this when releasing in Release
        client.Logger.LogDebug("Took {NsExecution}ns", Stopwatch.GetElapsedTime(startTime).TotalNanoseconds);
#endif

        MessageHandler handler = new(message, methodData, scope, client, mappedValues, name,
       _messageConditionBuilders);
        _ = handler.BuildModuleAndExecuteCommandAsync(); // Ignore error, this should be a fire and forget
    }
}
