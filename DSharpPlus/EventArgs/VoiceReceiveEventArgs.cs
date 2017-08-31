using System.Collections.Generic;
using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs
{
    /// <summary>
    /// Represents arguments for VoiceReceived events.
    /// </summary>
    public class VoiceReceiveEventArgs : DiscordEventArgs
    {
        /// <summary>
        /// Gets the SSRC of the audio source.
        /// </summary>
        public uint SSRC { get; internal set; }
        
        /// <summary>
        /// Gets the user that sent the audio data.
        /// </summary>
        public DiscordUser User { get; internal set; }

        /// <summary>
        /// Gets the received data.
        /// </summary>
        public IReadOnlyList<byte> Voice { get; internal set; }

        /// <summary>
        /// Gets the length of the received data.
        /// </summary>
        public int VoiceLength { get; internal set; }

        internal VoiceReceiveEventArgs(DiscordClient client) : base(client) { }
    }
}
