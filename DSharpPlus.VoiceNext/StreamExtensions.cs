// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2023

 DSharpPlus Contributors
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
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DSharpPlus.VoiceNext
{
    public static class StreamExtensions
    {
        /// <summary>
        /// Asynchronously reads the bytes from the current stream and writes them to the specified <see cref="VoiceTransmitSink"/>.
        /// </summary>
        /// <param name="source">The source <see cref="Stream"/></param>
        /// <param name="destination">The target <see cref="VoiceTransmitSink"/></param>
        /// <param name="bufferSize">The size, in bytes, of the buffer. This value must be greater than zero. If <see langword="null"/>, defaults to the packet size specified by <paramref name="destination"/>.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns></returns>
        public static async Task CopyToAsync(this Stream source, VoiceTransmitSink destination, int? bufferSize = null, CancellationToken cancellationToken = default)
        {
            // adapted from CoreFX
            // https://source.dot.net/#System.Private.CoreLib/Stream.cs,8048a9680abdd13b

            if (source is null)
                throw new ArgumentNullException(nameof(source));
            if (destination is null)
                throw new ArgumentNullException(nameof(destination));
            if (bufferSize != null && bufferSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(bufferSize), bufferSize, "bufferSize cannot be less than or equal to zero");

            var bufferLength = bufferSize ?? destination.SampleLength;
            var buffer = ArrayPool<byte>.Shared.Rent(bufferLength);
            try
            {
                int bytesRead;
                while ((bytesRead = await source.ReadAsync(buffer, 0, bufferLength, cancellationToken).ConfigureAwait(false)) != 0)
                {
                    await destination.WriteAsync(new ReadOnlyMemory<byte>(buffer, 0, bytesRead), cancellationToken).ConfigureAwait(false);
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
    }
}
