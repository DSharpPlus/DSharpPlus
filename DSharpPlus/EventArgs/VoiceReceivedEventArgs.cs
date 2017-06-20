using System.Collections.Generic;

namespace DSharpPlus
{
    public class VoiceReceivedEventArgs : DiscordEventArgs
    {
        public uint SSRC { get; internal set; }
        /// <summary>
        /// User voice belongs to
        /// </summary>
        public DiscordUser User { get; internal set; }
        /// <summary>
        /// Received voice bytes
        /// </summary>
        public IReadOnlyList<byte> Voice { get; internal set; }
        /// <summary>
        /// Voice length
        /// </summary>
        public int VoiceLength { get; internal set; }

        internal VoiceReceivedEventArgs(DiscordClient client) : base(client) { }
    }
}
