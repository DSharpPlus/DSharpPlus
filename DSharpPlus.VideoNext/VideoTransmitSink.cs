using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.VoiceNext;
using DSharpPlus.VoiceNext.Codec;

namespace DSharpPlus.VideoNext
{
  /// <summary>
    /// Sink used to transmit audio data via <see cref="VoiceNextConnection"/>.
    /// </summary>
    public sealed class VideoTransmitSink : IDisposable
    {
        

        private VideoNextConnection Connection { get; }
        private int H264Duration { get; }
        private byte[] H264Buffer { get; }
        private Memory<byte> H264Memory { get; }
        private int H264BufferLength { get; set; }
        private SemaphoreSlim WriteSemaphore { get; }
        
        /// <summary>
        /// Gets the PCM sample duration for this sink.
        /// </summary>
        public int SampleDuration
            => this.H264Duration;

        /// <summary>
        /// Gets the length of the PCM buffer for this sink. 
        /// Written packets should adhere to this size, but the sink will adapt to fit.
        /// </summary>
        public int SampleLength
            => this.H264Buffer.Length;

        internal VideoTransmitSink(VideoNextConnection vnc)
        {
            this.Connection = vnc;
            this.H264Duration = 20;
            this.H264Buffer = new byte[200];
            this.H264Memory = this.H264Buffer.AsMemory();
            this.H264BufferLength = 0;
            this.WriteSemaphore = new SemaphoreSlim(1, 1);
        }

        /// <summary>
        /// Writes PCM data to the sink. The data is prepared for transmission, and enqueued.
        /// </summary>
        /// <param name="buffer">PCM data buffer to send.</param>
        /// <param name="offset">Start of the data in the buffer.</param>
        /// <param name="count">Number of bytes from the buffer.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        public async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
        {
            await WriteAsync(new ReadOnlyMemory<byte>(buffer, offset, count), cancellationToken);
        }

        /// <summary>
        /// Writes PCM data to the sink. The data is prepared for transmission, and enqueued.
        /// </summary>
        /// <param name="buffer">PCM data buffer to send.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        public async Task WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            await this.WriteSemaphore.WaitAsync(cancellationToken);

            try
            {
                var remaining = buffer.Length;
                var buffSpan = buffer;
                var pcmSpan = this.H264Memory;

                while (remaining > 0)
                {
                    var len = Math.Min(pcmSpan.Length - this.H264BufferLength, remaining);

                    var tgt = pcmSpan.Slice(this.H264BufferLength);
                    var src = buffSpan.Slice(0, len);

                    src.CopyTo(tgt);
                    this.H264BufferLength += len;
                    remaining -= len;
                    buffSpan = buffSpan.Slice(len);

                    if (this.H264BufferLength == this.H264Buffer.Length)
                    {
                        this.H264BufferLength = 0;

                        var packet = ArrayPool<byte>.Shared.Rent(H264Memory.Length);
                        var packetMemory = packet.AsMemory().Slice(0, H264Memory.Length);
                        H264Memory.CopyTo(packetMemory);

                        await Connection.EnqueuePacketAsync(new RawVideoPacket(H264Buffer, H264Duration, false));
                    }
                }
            }
            finally
            {
                this.WriteSemaphore.Release();
            }
        }

        /// <summary>
        /// Flushes the rest of the PCM data in this buffer to VoiceNext packet queue.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        public async Task FlushAsync(CancellationToken cancellationToken = default)
        {
            var pcm = this.H264Memory;
            Helpers.ZeroFill(pcm.Slice(this.H264BufferLength).Span);


            var packet = ArrayPool<byte>.Shared.Rent(pcm.Length);
            var packetMemory = packet.AsMemory().Slice(0, pcm.Length);
            pcm.CopyTo(packetMemory);

            await Connection.EnqueuePacketAsync(new RawVideoPacket(H264Buffer, H264Duration, false));

        }
        
        
        public void Dispose() => throw new NotImplementedException();
    }
}