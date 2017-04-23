using System;

namespace DSharpPlus
{
    public class VoiceServerUpdateEventArgs : EventArgs
    {
        internal DiscordClient Discord { get; set; }
        internal string VoiceToken { get; set; }
        public ulong GuildID { get; internal set; }
        public DiscordGuild Guild => this.Discord._rest_client.InternalGetGuildAsync(GuildID).GetAwaiter().GetResult();
        public string Endpoint { get; internal set; }
    }
}
