using System;

namespace DSharpPlus
{
    public class VoiceServerUpdateEventArgs : EventArgs
    {
        internal string VoiceToken;
        public ulong GuildID;
        public DiscordGuild Guild => DiscordClient.InternalGetGuild(GuildID).Result;
        public string Endpoint;
    }
}
