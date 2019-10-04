﻿namespace DSharpPlus.VoiceNext
{
    /// <summary>
    /// VoiceNext client configuration.
    /// </summary>
    public sealed class VoiceNextConfiguration
    {
        /// <summary>
        /// <para>Sets the audio format for Opus. This will determine the quality of the audio output.</para>
        /// <para>Defaults to <see cref="AudioFormat.Default"/>.</para>
        /// </summary>
        public AudioFormat AudioFormat { internal get; set; } = AudioFormat.Default;

#if !NETSTANDARD1_1
        /// <summary>
        /// <para>Sets whether incoming voice receiver should be enabled.</para>
        /// <para>Defaults to false.</para>
        /// </summary>
        public bool EnableIncoming { internal get; set; } = false;
#endif

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
            this.AudioFormat = new AudioFormat(other.AudioFormat.SampleRate, other.AudioFormat.ChannelCount, other.AudioFormat.VoiceApplication);
#if !NETSTANDARD1_1
            this.EnableIncoming = other.EnableIncoming;
#endif
        }
    }
}
