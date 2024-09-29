using System.Collections.Generic;
using DSharpPlus.Commands.Trees;
using DSharpPlus.EventArgs;

namespace DSharpPlus.Commands.EventArgs;

public sealed class ConfigureCommandsEventArgs : DiscordEventArgs
{
    public required List<CommandBuilder> CommandTrees { get; init; }
}
