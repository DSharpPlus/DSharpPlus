using System;

namespace DSharpPlus
{
    public class VoiceStateUpdateEventArgs : EventArgs
    {
        public ulong UserID;
        public DiscordUser User => DiscordClient.InternalGetUser(UserID.ToString()).Result;
        internal string SessionID;
    }
}
