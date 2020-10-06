using System;

namespace DSharpPlus.Interactivity.Enums
{
    [Flags]
    public enum PollBehaviour
    {
        /// <summary>
        /// Keeps emojis
        /// </summary>
        KeepEmojis = 0,
        /// <summary>
        /// Deletes Emojis
        /// </summary>
        DeleteEmojis = 1
    }
}
