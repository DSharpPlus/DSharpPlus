using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
