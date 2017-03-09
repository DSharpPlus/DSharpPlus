using System;

namespace DSharpPlus
{
    public class GuildDeleteEventArgs : EventArgs
    {
        public ulong ID { get; internal set; }
        public bool Unavailable { get; internal set; }
    }
}
