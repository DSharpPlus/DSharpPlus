using System;

namespace DSharpPlus
{
    public class GuildDeleteEventArgs : EventArgs
    {
        public ulong ID;
        public bool Unavailable;
    }
}
