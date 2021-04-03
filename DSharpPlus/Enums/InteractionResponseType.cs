namespace DSharpPlus
{
    /// <summary>
    /// Represents the type of interaction response
    /// </summary>
    public enum InteractionResponseType
    {
        /// <summary>
        /// Acknowledges a Ping.
        /// </summary>
        Pong = 1,
        
        /// <summary>
        /// Responds to the interaction with a message.
        /// </summary>
        ChannelMessageWithSource = 4,

        /// <summary>
        /// Acknowledges an interaction to edit to a response later. The user sees a "thinking" state.
        /// </summary>
        DeferredChannelMessageWithSource = 5
    }
}
