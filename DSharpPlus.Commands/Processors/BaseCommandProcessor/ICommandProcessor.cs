using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Commands.Converters;
using DSharpPlus.Commands.Trees;

namespace DSharpPlus.Commands.Processors;

public interface ICommandProcessor
{
    /// <summary>
    /// Processor specific context type. Context type which is provided on command invokation
    /// </summary>
    public Type ContextType { get; }

    /// <summary>
    /// A dictionary of argument converters indexed by the type they convert to.
    /// </summary>
    public IReadOnlyDictionary<Type, IArgumentConverter> Converters { get; }

    /// <summary>
    /// List of commands which are registered to this processor
    /// </summary>
    public IReadOnlyList<Command> Commands { get; }

    /// <summary>
    /// This method is called on initial setup and when the extension is refreshed.
    /// Register your needed event handlers here but use a mechanism to track
    /// if the inital setup was already done and if this call is only a refresh
    /// </summary>
    /// <param name="extension">Extension this processor belongs to</param>
    /// <returns></returns>
    public ValueTask ConfigureAsync(CommandsExtension extension);
}
