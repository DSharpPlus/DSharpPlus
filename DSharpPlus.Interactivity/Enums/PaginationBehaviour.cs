using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSharpPlus.Interactivity.Enums
{
    public enum PaginationDeletion
    {
        /// <summary>
        /// Deletes emojis
        /// </summary>
        DeleteEmojis = 0,
        /// <summary>
        /// Keeps emojis
        /// </summary>
        KeepEmojis = 1,
        /// <summary>
        /// Deletes message
        /// </summary>
        DeleteMessage = 2
    }

    public enum PaginationBehaviour
    {
        /// <summary>
        /// Wraps around indices (e.g. when the index in over the max, loop back to 0)
        /// </summary>
        WrapAround = 0,
        /// <summary>
        /// Disallows moving pas 0 and max indices
        /// </summary>
        Ignore = 1
    }
}
