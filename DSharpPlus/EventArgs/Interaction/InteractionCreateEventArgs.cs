using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs
{
    /// <summary>
    /// Represents arguments for <see cref="DiscordClient.InteractionCreated"/>
    /// </summary>
    public class InteractionCreateEventArgs : DiscordEventArgs
    {
        /// <summary>
        /// Gets the interaction data that was invoked.
        /// </summary>
        public DiscordInteraction Interaction { get; internal set; }
    }
}
