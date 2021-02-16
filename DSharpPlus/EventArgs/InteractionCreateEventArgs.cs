using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs
{
    /// <summary>
    /// Represents arguments for <see cref="DiscordClient.InternalConnectAsync"/>
    /// </summary>
    public class InteractionCreateEventArgs : DiscordEventArgs
    {
        /// <summary>
        /// Gets the slash command data that was invoked. 
        /// </summary>
        public DiscordInteraction Interaction { get; internal set; }
    }
}