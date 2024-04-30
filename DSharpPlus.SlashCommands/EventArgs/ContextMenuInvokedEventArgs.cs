namespace DSharpPlus.SlashCommands.EventArgs;

using DSharpPlus.AsyncEvents;

/// <summary>
/// Represents arguments for a <see cref="SlashCommandsExtension.ContextMenuInvoked"/>
/// </summary>
public sealed class ContextMenuInvokedEventArgs : AsyncEventArgs
{
    /// <summary>
    /// The context of the command.
    /// </summary>
    public ContextMenuContext Context { get; internal set; }
}
