using System.Collections.Generic;

namespace DSharpPlus
{
    public class VoiceReceivedEventArgs : DiscordEventArgs
    {
        public uint SSRC { get; internal set; }
        public ulong UserID { get; internal set; }
        public IReadOnlyList<byte> Voice { get; internal set; }
        public int VoiceLength { get; internal set; }

        internal VoiceReceivedEventArgs(uint ssrc, ulong userId, IReadOnlyList<byte> voice, int voiceLength, DiscordClient client)
            : base(client)
        {
            SSRC = ssrc;
            UserID = userId;
            Voice = voice;
            VoiceLength = voiceLength;
        }
    }
}
