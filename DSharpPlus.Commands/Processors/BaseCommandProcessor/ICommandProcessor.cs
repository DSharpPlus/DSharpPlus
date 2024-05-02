
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.AsyncEvents;
using DSharpPlus.Commands.Converters;

namespace DSharpPlus.Commands.Processors;
public interface ICommandProcessor
{
    public ValueTask ConfigureAsync(CommandsExtension extension);
}

public interface ICommandProcessor<T> : ICommandProcessor where T : AsyncEventArgs
{
    public IReadOnlyDictionary<Type, ConverterDelegate<T>> Converters { get; }
}
