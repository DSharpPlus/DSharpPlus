using DSharpPlus.Objects;

namespace DSharpPlus.EventArgs
{
    public class VoiceServerUpdateEventArgs : System.EventArgs
    {
        internal string VoiceToken;
        public ulong GuildID;
        public DiscordGuild Guild => DiscordClient.InternalGetGuild(GuildID).Result;
        public string Endpoint;
    }
}
