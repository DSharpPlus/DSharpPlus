
using DSharpPlus.AsyncEvents;

namespace DSharpPlus.SlashCommands.EventArgs;
/// <summary>
/// Represents the arguments for a <see cref="SlashCommandsExtension.SlashCommandInvoked"/> event.
/// </summary>
public sealed class SlashCommandInvokedEventArgs : AsyncEventArgs
{
    /// <summary>
    /// The context of the command.
    /// </summary>
    public InteractionContext Context { get; internal set; }
}
