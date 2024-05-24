using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.AsyncEvents;
using DSharpPlus.Commands.Converters;

namespace DSharpPlus.Commands.Processors;

public interface ICommandProcessor
{
    /// <summary>
    /// This method is called on initial setup and when the extension is refreshed.
    /// Register your needed event handlers here but use a mechanism to track
    /// if the inital setup was already done and if this call is only a refresh
    /// </summary>
    /// <param name="extension">Extension this processor belongs to</param>
    /// <returns></returns>
    public ValueTask ConfigureAsync(CommandsExtension extension);
    
    /// <summary>
    /// Processor specific context type. Context type which is provided on command invokation
    /// </summary>
    public Type ContextType { get; }
}

public interface ICommandProcessor<T> : ICommandProcessor where T : AsyncEventArgs
{
    public IReadOnlyDictionary<Type, ConverterDelegate<T>> Converters { get; }
}
