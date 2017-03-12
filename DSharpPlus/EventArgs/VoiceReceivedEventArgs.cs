using System;
using System.Collections.Generic;

namespace DSharpPlus
{
    public class VoiceReceivedEventArgs : EventArgs
    {
        public uint SSRC { get; internal set; }
        public ulong UserID { get; internal set; }
        public IReadOnlyList<byte> Voice { get; internal set; }
        public int VoiceLength { get; internal set; }

        internal VoiceReceivedEventArgs(uint ssrc, ulong userId, IReadOnlyList<byte> voice, int voiceLength)
        {
            SSRC = ssrc;
            UserID = userId;
            Voice = voice;
            VoiceLength = voiceLength;
        }
    }
}
