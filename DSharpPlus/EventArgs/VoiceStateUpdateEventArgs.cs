using DSharpPlus.Objects;

namespace DSharpPlus.EventArgs
{
    public class VoiceStateUpdateEventArgs : System.EventArgs
    {
        public ulong UserID;
        public DiscordUser User => DiscordClient.InternalGetUser(UserID.ToString()).Result;
        internal string SessionID;
    }
}
