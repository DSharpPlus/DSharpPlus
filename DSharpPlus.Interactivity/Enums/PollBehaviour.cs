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
        Default = 0,
        KeepEmojis = 1,
        DeleteEmojis = 2
    }
}
