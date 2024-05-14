using DSharpPlus.AsyncEvents;

namespace DSharpPlus.SlashCommands.EventArgs;

/// <summary>
/// Represents arguments for a <see cref="SlashCommandsExtension.ContextMenuExecuted"/>
/// </summary>
public sealed class ContextMenuExecutedEventArgs : AsyncEventArgs
{
    /// <summary>
    /// The context of the command.
    /// </summary>
    public ContextMenuContext Context { get; internal set; }
}
