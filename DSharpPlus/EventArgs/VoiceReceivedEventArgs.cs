using System.Collections.Generic;

namespace DSharpPlus
{
    public class VoiceReceivedEventArgs : DiscordEventArgs
    {
        public uint SSRC { get; internal set; }
        public DiscordUser User { get; internal set; }
        public IReadOnlyList<byte> Voice { get; internal set; }
        public int VoiceLength { get; internal set; }

        internal VoiceReceivedEventArgs(DiscordClient client) : base(client) { }
    }
}
