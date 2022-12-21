using Emzi0767.Utilities;

namespace DSharpPlus.SlashCommands.EventArgs;

/// <summary>
/// Represents the arguments for a <see cref="SlashCommandsExtension.SlashCommandExecuted"/> event.
/// </summary>
public sealed class SlashCommandExecutedEventArgs : AsyncEventArgs
{
    /// <summary>
    /// The context of the command.
    /// </summary>
    public InteractionContext Context { get; internal set; }
}
