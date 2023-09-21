namespace DSharpPlus.CommandsNext
{
    /// <summary>
    /// Represents arguments for <see cref="CommandsNextExtension.CommandExecuted"/> event.
    /// </summary>
    public class CommandExecutionEventArgs : CommandEventArgs
    {
        /// <summary>
        /// Gets the command that was executed.
        /// </summary>
        public new Command Command
            => this.Context.Command!;
    }
}
