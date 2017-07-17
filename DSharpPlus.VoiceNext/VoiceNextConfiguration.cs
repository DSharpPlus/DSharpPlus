using DSharpPlus.VoiceNext.Codec;

namespace DSharpPlus.VoiceNext
{
    /// <summary>
    /// VoiceNext client configuration.
    /// </summary>
    public sealed class VoiceNextConfiguration
    {
        /// <summary>
        /// Sets the encoding settings for this client. This decides whether the encoder will favour quality or smaller bandwidth.
        /// </summary>
        public VoiceApplication VoiceApplication { internal get; set; } = VoiceApplication.Music;

        /// <summary>
        /// Sets whether incoming voice receiver should be enabled.
        /// </summary>
        public bool EnableIncoming { internal get; set; } = false;
    }
}
