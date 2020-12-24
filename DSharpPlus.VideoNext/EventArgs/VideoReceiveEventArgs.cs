using System;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.VoiceNext;

namespace DSharpPlus.VideoNext.Entities
{
    /// <summary>
    /// Represents arguments for VoiceReceived events.
    /// </summary>
    public class VideoReceiveEventArgs : DiscordEventArgs
    {
        /// <summary>
        /// Gets the SSRC of the audio source.
        /// </summary>
        public uint SSRC { get; internal set; }

#pragma warning disable CS8632

        /// <summary>
        /// Gets the user that sent the audio data.
        /// </summary>
        public DiscordUser? User { get; internal set; }

#pragma warning restore

        /// <summary>
        /// Gets the received voice data, decoded to PCM format.
        /// </summary>
        public ReadOnlyMemory<byte> H264Data { get; internal set; }
        
        /// <summary>
        /// Gets the millisecond duration of the PCM audio sample.
        /// </summary>
        public int AudioDuration { get; internal set; }

        internal VideoReceiveEventArgs() : base() { }
    }
}