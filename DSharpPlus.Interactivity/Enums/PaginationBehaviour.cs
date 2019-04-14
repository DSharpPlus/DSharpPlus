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
        /// Defaults to DeleteEmojis
        /// </summary>
        Default = 0,
        /// <summary>
        /// Deletes emojis
        /// </summary>
        DeleteEmojis = 1,
        /// <summary>
        /// Keeps emojis
        /// </summary>
        KeepEmojis = 2,
        /// <summary>
        /// Deletes message
        /// </summary>
        DeleteMessage = 3
    }

    public enum PaginationBehaviour
    {
        /// <summary>
        /// Defaults to WrapAround
        /// </summary>
        Default = 0,
        /// <summary>
        /// Wraps around indices (e.g. when the index in over the max, loop back to 0)
        /// </summary>
        WrapAround = 1,
        /// <summary>
        /// Disallows moving pas 0 and max indices
        /// </summary>
        Ignore = 2
    }
}
