using System;

namespace DSharpPlus
{
    public class UserUpdateEventArgs : EventArgs
    {
        public DiscordUser User { get; internal set; }
    }
}
