using System;

namespace DSharpPlus
{
    public static class MessageFlagExtensions
    {
        /// <summary>
        /// Calculates whether these message flags contain a specific flag.
        /// </summary>
        /// <param name="baseFlags">The existing flags.</param>
        /// <param name="flag">The flags to search for.</param>
        /// <returns></returns>
        public static bool HasMessageFlag(this MessageFlags baseFlags, MessageFlags flag) => (baseFlags & flag) == flag;
    }

    /// <summary>
    /// Represents additional features of a message.
    /// </summary>
    [Flags]
    public enum MessageFlags
    {
        /// <summary>
        /// Whether this message is the original message that was published from a news channel to subscriber channels.
        /// </summary>
        Crossposted = 1 << 0,

        /// <summary>
        /// Whether this message is crossposted (automatically posted in a subscriber channel).
        /// </summary>
        IsCrosspost = 1 << 1,

        /// <summary>
        /// Whether any embeds in the message are hidden.
        /// </summary>
        SuppressedEmbeds = 1 << 2
    }
}
