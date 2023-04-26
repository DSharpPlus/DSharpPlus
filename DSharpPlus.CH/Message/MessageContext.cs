namespace DSharpPlus.CH.Message
{
    /// <summary>
    /// This is only used in middlewares
    /// </summary>
    public class MessageContext
    {
        /// <summary>
        /// The Discord message.
        /// </summary>
        public required Entities.DiscordMessage Message { get; set; }
        /// <summary>
        /// Data related to the command module.
        /// </summary>
        public required MessageCommandData Data { get; set; }
    }
}