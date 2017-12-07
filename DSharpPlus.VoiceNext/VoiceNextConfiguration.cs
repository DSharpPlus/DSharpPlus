using DSharpPlus.VoiceNext.Codec;

namespace DSharpPlus.VoiceNext
{
    /// <summary>
    /// VoiceNext client configuration.
    /// </summary>
    public sealed class VoiceNextConfiguration
    {
        /// <summary>
        /// <para>Sets the encoding settings for this client. This decides whether the encoder will favour quality or smaller bandwidth.</para>
        /// <para>Defaults to <see cref="VoiceApplication.Music"/>.</para>
        /// </summary>
        public VoiceApplication VoiceApplication { internal get; set; } = VoiceApplication.Music;

        /// <summary>
        /// <para>Sets whether incoming voice receiver should be enabled.</para>
        /// <para>Defaults to false.</para>
        /// </summary>
        public bool EnableIncoming { internal get; set; } = false;

        /// <summary>
        /// Creates a new instance of <see cref="VoiceNextConfiguration"/>.
        /// </summary>
        public VoiceNextConfiguration() { }

        /// <summary>
        /// Creates a new instance of <see cref="VoiceNextConfiguration"/>, copying the properties of another configuration.
        /// </summary>
        /// <param name="other">Configuration the properties of which are to be copied.</param>
        public VoiceNextConfiguration(VoiceNextConfiguration other)
        {
            this.VoiceApplication = other.VoiceApplication;
            this.EnableIncoming = other.EnableIncoming;
        }
    }
}
