using System;

namespace DSharpPlus
{
    public class UserSpeakingEventArgs : EventArgs
    {
        public ulong UserID { get; internal set; }
        public uint SSRC { get; internal set; }
        public bool Speaking { get; internal set; }
    }
}
