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
        /// Defaults to DeleteEmojis
        /// </summary>
        Default = 0,
        /// <summary>
        /// Keeps emojis
        /// </summary>
        KeepEmojis = 1,
        /// <summary>
        /// Deletes Emojis
        /// </summary>
        DeleteEmojis = 2
    }
}
