using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DSharpPlus
{
    public class VoiceReceivedEventArgs : EventArgs
    {
        public uint SSRC { get; internal set; }
        public ulong UserID { get; internal set; }
        public IReadOnlyCollection<byte> Voice { get; internal set; }
        public int VoiceLength { get; internal set; }

        internal VoiceReceivedEventArgs(uint ssrc, ulong userId, IReadOnlyCollection<byte> voice, int voiceLength)
        {
            SSRC = ssrc;
            UserID = userId;
            Voice = voice;
            VoiceLength = voiceLength;
        }
    }
}
