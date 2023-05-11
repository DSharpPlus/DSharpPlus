using System.Diagnostics;
using System.Text;
using DSharpPlus.UnifiedCommands.Message.Conditions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DSharpPlus.UnifiedCommands.Message.Internals;

internal class MessageFactory
{
    private readonly MessageTree _commands = new();
    private List<Func<IServiceProvider, IMessageCondition>> _messageConditionBuilders = new();
    private IServiceProvider _services;

    internal void AddCommand(string name, MessageMethodData data) => _commands.Branches!.Add(name, new(data));
    internal void AddBranch(string name, MessageTree branch) => _commands.Branches!.Add(name, branch);

    internal MessageTree? GetBranch(string name) =>
        _commands.Branches!.TryGetValue(name, out MessageTree? result) ? result : null;

    public MessageFactory(IServiceProvider services)
        => _services = services;

    internal void AddMessageConditionBuilder(Func<IServiceProvider, IMessageCondition> func)
        => _messageConditionBuilders.Add(func);

    internal void ConstructAndExecuteCommand(Entities.DiscordMessage message,
        DiscordClient client, ref ReadOnlySpan<char> args, List<Range> argsRange)
    {
        long startTime = Stopwatch.GetTimestamp();

        Index end = 0;
        int it = 0;

        MessageTree? tree = null;

        foreach (Range argRange in argsRange)
        {
            ReadOnlySpan<char> arg = args[argRange.Start..argRange.End];
            if (tree is null)
            {
                if (_commands.Branches!.TryGetValue(arg.ToString(), out MessageTree? res))
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

            if (tree.Branches is not null && tree.Branches.TryGetValue(arg.ToString(), out MessageTree? res2))
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

        int positionalArgumentPosition = 0;
        Dictionary<string, string?> mappedValues = new();
        foreach (MessageParameterData data in tree.Data.Parameters)
        {
            if (options.TryGetValue(data.Name, out Range? value) || (data.ShorthandOptionName is not null &&
                                                                     options.TryGetValue(data.ShorthandOptionName,
                                                                         out value)))
            {
                if (data.Type == MessageParameterDataType.Bool && value is not null)
                {
                    string strValue = args[value.Value.Start..value.Value.End].ToString();
                    Task.Run(async () => await _services.GetRequiredService<IErrorHandler>().HandleConversionAsync(
                        new InvalidMessageConversionError
                        {
                            Name = name,
                            IsPositionalArgument = data.IsPositionalArgument,
                            Value = strValue,
                            Type = InvalidMessageConversionType.BoolShouldNotHaveValue,
                        }, message));
                    return;
                }
                else if (data.Type != MessageParameterDataType.Bool && value is null)
                {
                    Task.Run(async () => await new DefaultErrorHandler().HandleConversionAsync(
                        new InvalidMessageConversionError
                        {
                            Name = name,
                            IsPositionalArgument = data.IsPositionalArgument,
                            Value = "",
                            Type = InvalidMessageConversionType.NoValueProvided,
                        }, message));
                    return;
                }
                else if (data.Type == MessageParameterDataType.User
                         || data.Type == MessageParameterDataType.Channel
                         || data.Type == MessageParameterDataType.Member)
                {
                    ReadOnlySpan<char> span = args[value!.Value.Start..value.Value.End];
                    if (span.StartsWith("<@") || span.StartsWith("<#"))
                    {
                        ReadOnlySpan<char> formattedSpan = span is [_, _, '!', ..] ? span[3..^1] : span[2..^1];
                        mappedValues.Add(data.Name, formattedSpan.ToString());
                        continue;
                    }

                    mappedValues.Add(data.Name, span.ToString()); // MIGHT be a ID.
                }
                else if (data.Type == MessageParameterDataType.Role)
                {
                    ReadOnlySpan<char> span = args[value!.Value.Start..value.Value.End];
                    ReadOnlySpan<char> formattedSpan = span[2..^1];
                    mappedValues.Add(data.Name, formattedSpan.ToString());
                }
                else if (data.Type != MessageParameterDataType.Bool)
                {
                    if (value is null)
                    {
                        mappedValues.Add(data.Name, null);
                    }
                    else
                    {
                        ReadOnlySpan<char> span = args[value.Value.Start..value.Value.End];
                        mappedValues.Add(data.Name, span.ToString());
                    }
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
                    if (data.WillConsumeRestOfArguments)
                    {
                        StringBuilder stringBuilder = new();
                        for (;
                             positionalArgumentPosition < arguments.Count;
                             positionalArgumentPosition++)
                        {
                            Range range2 = arguments[positionalArgumentPosition];
                            stringBuilder.Append(args[range2.Start..range2.End]);
                            stringBuilder.Append(' ');
                        }

                        stringBuilder.Remove(stringBuilder.Length - 1, 1);

                        mappedValues.Add(data.Name, stringBuilder.ToString());
                        continue;
                    }

                    Range range = arguments[positionalArgumentPosition];
                    positionalArgumentPosition++;
                    ReadOnlySpan<char> span = args[range.Start..range.End];

                    if (data.Type == MessageParameterDataType.User
                        || data.Type == MessageParameterDataType.Channel
                        || data.Type == MessageParameterDataType.Member)
                    {
                        if (!span.StartsWith("<@") || !span.StartsWith("<#"))
                        {
                            mappedValues.Add(data.Name, span.ToString()); // MIGHT be a ID.
                            continue;
                        }

                        ReadOnlySpan<char> formattedSpan = span is [_, _, '!', ..] ? span[2..^1] : span[1..^1];
                        mappedValues.Add(data.Name, formattedSpan.ToString());
                    }
                    else if (data.Type == MessageParameterDataType.Role)
                    {
                        ReadOnlySpan<char> formattedSpan = span[2..^1];
                        mappedValues.Add(data.Name, formattedSpan.ToString());
                    }
                    else if (data.Type != MessageParameterDataType.Bool)
                    {
                        mappedValues.Add(data.Name, span.ToString());
                    }
                    else
                    {
                        mappedValues.Add(data.Name, "true");
                    }
                }
                else if (data.Type != MessageParameterDataType.Bool)
                {
                    if (value is null)
                    {
                        mappedValues.Add(data.Name, null);
                    }
                    else
                    {
                        ReadOnlySpan<char> span = args[value.Value.Start..value.Value.End];
                        mappedValues.Add(data.Name, span.ToString());
                    }
                }
                else
                {
                    mappedValues.Add(data.Name, "false");
                }
            }
        }

        client.Logger.LogDebug("Took {NsExecution}ns", Stopwatch.GetElapsedTime(startTime).TotalNanoseconds);

        IServiceScope scope = _services.CreateScope(); // This will need to be disposed later somehow.

        MessageHandler handler = new(message, tree.Data, scope, client, mappedValues, name,
            _messageConditionBuilders);
        _ = handler.BuildModuleAndExecuteCommandAsync();
    }
}
