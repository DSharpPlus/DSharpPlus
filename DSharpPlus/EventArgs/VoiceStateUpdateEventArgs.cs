using System;

namespace DSharpPlus
{
    public class VoiceStateUpdateEventArgs : EventArgs
    {
        internal DiscordClient Discord { get; set; }
        public ulong UserID { get; internal set; }
        public ulong GuildID { get; internal set; }
        public DiscordUser User => this.Discord._rest_client.InternalGetUser(UserID.ToString()).GetAwaiter().GetResult();
        internal string SessionID { get; set; }
    }
}
