using DSharpPlus.VoiceNext.Codec;

namespace DSharpPlus.VoiceNext
{
    /// <summary>
    /// VoiceNext client configuration.
    /// </summary>
    public sealed class VoiceNextConfiguration
    {
        /// <summary>
        /// Gets or sets the encoding settings for this client. This decides whether the encoder will favour quality or smaller bandwidth.
        /// </summary>
        public VoiceApplication VoiceApplication { get; set; }
    }
}
