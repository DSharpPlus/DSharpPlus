// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2022 DSharpPlus Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

namespace DSharpPlus.VoiceNext
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

        /// <summary>
        /// <para>Sets whether incoming voice receiver should be enabled.</para>
        /// <para>Defaults to false.</para>
        /// </summary>
        public bool EnableIncoming { internal get; set; } = false;

        /// <summary>
        /// <para>Sets the size of the packet queue.</para>
        /// <para>Defaults to 25 or ~500ms.</para>
        /// </summary>
        public int PacketQueueSize { internal get; set; } = 25;

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
            this.EnableIncoming = other.EnableIncoming;
        }
    }
}
