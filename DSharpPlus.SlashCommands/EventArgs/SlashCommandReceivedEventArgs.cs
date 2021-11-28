using Emzi0767.Utilities;

namespace DSharpPlus.SlashCommands.EventArgs
{
    /// <summary>
    /// Represents the arguments for a <see cref="SlashCommandsExtension.SlashCommandReceived"/> event.
    /// </summary>
    public sealed class SlashCommandReceivedEventArgs : AsyncEventArgs
    {
        /// <summary>
        /// The context of the command.
        /// </summary>
        public InteractionContext Context { get; internal set; }
    }
}
