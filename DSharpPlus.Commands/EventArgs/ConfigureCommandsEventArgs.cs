using System.Collections.Generic;
using DSharpPlus.AsyncEvents;
using DSharpPlus.Commands.Trees;

namespace DSharpPlus.Commands.EventArgs;

public sealed class ConfigureCommandsEventArgs : AsyncEventArgs
{
    public required List<CommandBuilder> CommandTrees { get; init; }
}
