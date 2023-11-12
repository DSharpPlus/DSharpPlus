namespace DSharpPlus.CommandAll.Processors;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.AsyncEvents;
using DSharpPlus.CommandAll.Converters;

public interface ICommandProcessor
{
    public Task ConfigureAsync(CommandAllExtension extension);
}

public interface ICommandProcessor<T> : ICommandProcessor where T : AsyncEventArgs
{
    public IReadOnlyDictionary<Type, ConverterDelegate<T>> Converters { get; }
}
