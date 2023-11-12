namespace DSharpPlus.Commands.Processors;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.AsyncEvents;
using DSharpPlus.Commands.Converters;

public interface ICommandProcessor
{
    public Task ConfigureAsync(CommandsExtension extension);
}

public interface ICommandProcessor<T> : ICommandProcessor where T : AsyncEventArgs
{
    public IReadOnlyDictionary<Type, ConverterDelegate<T>> Converters { get; }
}
