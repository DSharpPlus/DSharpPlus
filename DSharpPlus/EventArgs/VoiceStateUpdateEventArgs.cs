using System;

namespace DSharpPlus
{
    public class VoiceStateUpdateEventArgs : EventArgs
    {
        public ulong UserID { get; internal set; }
        public DiscordUser User => DiscordClient.InternalGetUser(UserID.ToString()).Result;
        internal string SessionID { get; set; }
    }
}
