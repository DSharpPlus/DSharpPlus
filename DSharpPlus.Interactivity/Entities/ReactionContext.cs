using DSharpPlus.Entities;

namespace DSharpPlus.Interactivity
{
    /// <summary>
    /// Context of a reaction
    /// </summary>
    public class ReactionContext
    {
        /// <summary>
        /// Channel reaction was sent in
        /// </summary>
        public DiscordChannel Channel { get; internal set; }

        /// <summary>
        /// User that sent a reaction
        /// </summary>
        public DiscordUser User { get; internal set; }

        /// <summary>
        /// Emoji that was sent
        /// </summary>
        public DiscordEmoji Emoji { get; internal set; }

        /// <summary>
        /// Message that was sent
        /// </summary>
        public DiscordMessage Message { get; internal set; }

        /// <summary>
        /// Guild this all found place in
        /// </summary>
        public DiscordGuild Guild 
            => Channel.Guild;

        /// <summary>
        /// Responsible interactivity extension
        /// </summary>
        public InteractivityExtension Interactivity { get; internal set; }

        /// <summary>
        /// Client that was listened to
        /// </summary>
        public DiscordClient Client 
            => Interactivity.Client;
    }
}
