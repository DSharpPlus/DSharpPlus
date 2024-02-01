namespace DSharpPlus.Commands.EventArgs;

using DSharpPlus.AsyncEvents;
using DSharpPlus.Commands.Trees;

/// <summary>
/// Represents arguments for <see cref="CommandsExtension.CommandExecuted"/> event.
/// </summary>
public sealed class CommandExecutedEventArgs : AsyncEventArgs
{
    /// <summary>
    /// Gets the context in which the command was executed.
    /// </summary>
    public required CommandContext Context { get; init; }
    
    /// <summary>
    /// Gets the command object that was executed.
    /// </summary>
    public required object? CommandObject { get; init; }
}
