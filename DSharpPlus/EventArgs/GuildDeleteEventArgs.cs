using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSharpPlus
{
    public class GuildDeleteEventArgs : EventArgs
    {
        public ulong ID;
        public bool Unavailable;
    }
}
