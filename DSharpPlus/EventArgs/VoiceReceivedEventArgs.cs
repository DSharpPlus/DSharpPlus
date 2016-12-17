namespace DSharpPlus.EventArgs
{
    public class VoiceReceivedEventArgs : System.EventArgs
    {
        public uint SSRC { get; internal set; }
        public ulong UserID { get; internal set; }
        public byte[] Voice { get; internal set; }
        public int VoiceLength { get; internal set; }

        internal VoiceReceivedEventArgs(uint ssrc, ulong userId, byte[] voice, int voiceLength)
        {
            SSRC = ssrc;
            UserID = userId;
            Voice = voice;
            VoiceLength = voiceLength;
        }
    }
}
