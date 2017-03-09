using System;

namespace DSharpPlus
{
    public class UserSettingsUpdateEventArgs : EventArgs
    {
        public DiscordUser User { get; internal set; }
    }
}
