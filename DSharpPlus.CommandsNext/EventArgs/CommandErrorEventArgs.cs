using System;

namespace DSharpPlus.CommandsNext;

/// <summary>
/// Represents arguments for <see cref="CommandsNextExtension.CommandErrored"/> event.
/// </summary>
public class CommandErrorEventArgs : CommandEventArgs
{
    public Exception Exception { get; internal set; } = null!;
}
