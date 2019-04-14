using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSharpPlus.Interactivity.Enums
{
    public enum PaginationDeletion
    {
        Default = 0,
        DeleteEmojis = 1,
        KeepEmojis = 2,
        DeleteMessage = 3
    }

    public enum PaginationBehaviour
    {
        Default = 0,
        WrapAround = 1,
        Ignore = 2
    }
}
