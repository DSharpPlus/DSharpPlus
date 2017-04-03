using System;

namespace DSharpPlus
{
    public class UserUpdateEventArgs : EventArgs
    {
        public DiscordUser User { get; internal set; }
        public DiscordUser UserBefore { get; internal set; }
    }
}
