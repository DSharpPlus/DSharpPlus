using System;
using DSharpPlus.Entities;

namespace DSharpPlus.Interactivity
{
    /// <summary>
    /// Context about a typing user
    /// </summary>
    public class TypingContext
    {
        /// <summary>
        /// User that was typing
        /// </summary>
        public DiscordUser User { get; internal set; }

        /// <summary>
        /// Channel the user was typing in
        /// </summary>
        public DiscordChannel Channel { get; internal set; }

        /// <summary>
        /// WHen the user started typing
        /// </summary>
        public DateTimeOffset StartedAt { get; internal set; }

        /// <summary>
        /// Guild the user was typing in
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
