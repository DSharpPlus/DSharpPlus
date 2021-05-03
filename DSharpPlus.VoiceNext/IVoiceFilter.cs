// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2021 DSharpPlus Contributors
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

using System;

namespace DSharpPlus.VoiceNext
{
    /// <summary>
    /// Represents a filter for PCM data. PCM data submitted through a <see cref="VoiceTransmitSink"/> will be sent through all installed instances of <see cref="IVoiceFilter"/> first.
    /// </summary>
    public interface IVoiceFilter
    {
        /// <summary>
        /// Transforms the supplied PCM data using this filter.
        /// </summary>
        /// <param name="pcmData">PCM data to transform. The transformation happens in-place.</param>
        /// <param name="pcmFormat">Format of the supplied PCM data.</param>
        /// <param name="duration">Millisecond duration of the supplied PCM data.</param>
        void Transform(Span<short> pcmData, AudioFormat pcmFormat, int duration);
    }
}
