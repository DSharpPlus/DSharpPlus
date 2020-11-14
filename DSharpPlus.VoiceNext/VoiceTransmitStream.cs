using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using DSharpPlus.VoiceNext.Codec;
using DSharpPlus.VoiceNext.Entities;

namespace DSharpPlus.VoiceNext
{
    /// <summary>
    /// Stream used to transmit audio data via <see cref="VoiceNextConnection"/>.
    /// </summary>
    public sealed class VoiceTransmitStream : Stream
    {
        /// <summary>
        /// Gets whether this stream can be read from. Always false.
        /// </summary>
        public override bool CanRead => false;

        /// <summary>
        /// Gets whether this stream can be seeked. Always false.
        /// </summary>
        public override bool CanSeek => false;

        /// <summary>
        /// Gets whether this stream can be written to. Always false.
        /// </summary>
        public override bool CanWrite => true;

        /// <summary>
        /// Gets the length of the data in this stream. Always throws <see cref="InvalidOperationException"/>.
        /// </summary>
        public override long Length => throw new InvalidOperationException($"{this.GetType()} cannot have a length.");

        /// <summary>
        /// Gets the position in this stream. Always throws <see cref="InvalidOperationException"/>.
        /// </summary>
        public override long Position
        {
            get => throw new InvalidOperationException($"Cannot get position of {this.GetType()}.");
            set => throw new InvalidOperationException($"Cannot seek {this.GetType()}.");
        }

        /// <summary>
        /// Gets the PCM sample duration for this stream.
        /// </summary>
        public int SampleDuration
            => this.PcmBufferDuration;

        /// <summary>
        /// Gets or sets the volume modifier for this stream. Changing this will alter the volume of the output. 1.0 is 100%.
        /// </summary>
        public double VolumeModifier
        {
            get => this._volume;
            set
            {
                if (value < 0 || value > 2.5)
                    throw new ArgumentOutOfRangeException(nameof(value), "Volume needs to be between 0% and 250%.");

                this._volume = value;
            }
        }
        private double _volume = 1.0;

        private VoiceNextConnection Connection { get; }
        private int PcmBufferDuration { get; }
        private byte[] PcmBuffer { get; }
        private Memory<byte> PcmMemory { get; }
        private int PcmBufferLength { get; set; }
        private SemaphoreSlim WriteSemaphore { get; }

        private List<IVoiceFilter> Filters { get; }

        internal VoiceTransmitStream(VoiceNextConnection vnc, int pcmBufferDuration)
        {
            this.Connection = vnc;
            this.PcmBufferDuration = pcmBufferDuration;
            this.PcmBuffer = new byte[vnc.AudioFormat.CalculateSampleSize(pcmBufferDuration)];
            this.PcmMemory = this.PcmBuffer.AsMemory();
            this.PcmBufferLength = 0;
            this.WriteSemaphore = new SemaphoreSlim(1, 1);
            this.Filters = new List<IVoiceFilter>();
        }

        /// <summary>
        /// Reads from the stream. Throws <see cref="InvalidOperationException"/>.
        /// </summary>
        /// <param name="buffer">Buffer to read to.</param>
        /// <param name="offset">Offset to read to.</param>
        /// <param name="count">Number of bytes to read.</param>
        /// <returns>Number of bytes read.</returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new InvalidOperationException($"Cannot read from {this.GetType()}.");
        }

        /// <summary>
        /// Seeks the stream. Throws <see cref="InvalidOperationException"/>.
        /// </summary>
        /// <param name="offset">Offset to seek to.</param>
        /// <param name="origin">Origin of seeking.</param>
        /// <returns>New position in the stream.</returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new InvalidOperationException($"Cannot seek {this.GetType()}.");
        }

        /// <summary>
        /// Sets length of this stream. Throws <see cref="InvalidOperationException"/>.
        /// </summary>
        /// <param name="value">Length to set.</param>
        public override void SetLength(long value)
        {
            throw new InvalidOperationException($"Cannot set length of {this.GetType()}.");
        }

        /// <summary>
        /// DON'T USE THIS, USE <see cref="WriteAsync(byte[], int, int, CancellationToken)"/>!
        /// </summary>
        public override void Write(byte[] buffer, int offset, int count)
        {
            WriteAsync(buffer, offset, count, default).GetAwaiter().GetResult();
        }

        /// <summary>
        /// DON'T USE THIS, USE <see cref="FlushAsync(CancellationToken)"/>!
        /// </summary>
        public override void Flush()
        {
            FlushAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Writes PCM data to the stream. The data is prepared for transmission, and enqueued.
        /// </summary>
        /// <param name="buffer">PCM data buffer to send.</param>
        /// <param name="offset">Start of the data in the buffer.</param>
        /// <param name="count">Number of bytes from the buffer.</param>
        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            await WriteAsync(new ReadOnlyMemory<byte>(buffer, offset, count), cancellationToken);
        }

        /// <summary>
        /// Writes PCM data to the stream. The data is prepared for transmission, and enqueued.
        /// </summary>
        /// <param name="buffer">PCM data buffer to send.</param>
        /// <param name="cancellationToken"></param>
        public async Task WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            await this.WriteSemaphore.WaitAsync(cancellationToken);

            var remaining = buffer.Length;
            var buffSpan = buffer;
            var pcmSpan = this.PcmMemory;

            while (remaining > 0)
            {
                var len = Math.Min(pcmSpan.Length - this.PcmBufferLength, remaining);

                var tgt = pcmSpan.Slice(this.PcmBufferLength);
                var src = buffSpan.Slice(0, len);

                src.CopyTo(tgt);
                this.PcmBufferLength += len;
                remaining -= len;
                buffSpan = buffSpan.Slice(len);

                if (this.PcmBufferLength == this.PcmBuffer.Length)
                {
                    ApplyFiltersSync(pcmSpan);

                    this.PcmBufferLength = 0;

                    var packet = ArrayPool<byte>.Shared.Rent(PcmMemory.Length);
                    var packetMemory = packet.AsMemory().Slice(0, PcmMemory.Length);
                    PcmMemory.CopyTo(packetMemory);

                    await Connection.EnqueuePacketAsync(new RawVoicePacket(packetMemory, PcmBufferDuration, false, packet), cancellationToken);
                }
            }

            this.WriteSemaphore.Release();
        }

        /// <summary>
        /// Flushes the rest of the PCM data in this buffer to VoiceNext packet queue.
        /// </summary>
        public override async Task FlushAsync(CancellationToken cancellationToken = default)
        {
            var pcm = this.PcmMemory;
            Helpers.ZeroFill(pcm.Slice(this.PcmBufferLength).Span);

            ApplyFiltersSync(pcm);

            var packet = ArrayPool<byte>.Shared.Rent(pcm.Length);
            var packetMemory = packet.AsMemory().Slice(0, pcm.Length);
            pcm.CopyTo(packetMemory);

            await Connection.EnqueuePacketAsync(new RawVoicePacket(packetMemory, PcmBufferDuration, false, packet), cancellationToken);
        }

        /// <summary>
        /// Pauses playback.
        /// </summary>
        public void Pause()
            => this.Connection.Pause();

        /// <summary>
        /// Resumes playback.
        /// </summary>
        /// <returns></returns>
        public async Task ResumeAsync()
            => await this.Connection.ResumeAsync().ConfigureAwait(false);

        /// <summary>
        /// Gets the collection of installed PCM filters, in order of their execution.
        /// </summary>
        /// <returns>Installed PCM filters, in order of execution.</returns>
        public IEnumerable<IVoiceFilter> GetInstalledFilters()
        {
            foreach (var filter in this.Filters)
                yield return filter;
        }

        /// <summary>
        /// Installs a new PCM filter, with specified execution order.
        /// </summary>
        /// <param name="filter">Filter to install.</param>
        /// <param name="order">Order of the new filter. This determines where the filter will be inserted in the filter stream.</param>
        public void InstallFilter(IVoiceFilter filter, int order = int.MaxValue)
        {
            if (filter == null)
                throw new ArgumentNullException(nameof(filter));

            if (order < 0)
                throw new ArgumentOutOfRangeException(nameof(order), "Filter order must be greater than or equal to 0.");

            lock (this.Filters)
            {
                var filters = this.Filters;
                if (order >= filters.Count)
                    filters.Add(filter);
                else
                    filters.Insert(order, filter);
            }
        }

        /// <summary>
        /// Uninstalls an installed PCM filter.
        /// </summary>
        /// <param name="filter">Filter to uninstall.</param>
        /// <returns>Whether the filter was uninstalled.</returns>
        public bool UninstallFilter(IVoiceFilter filter)
        {
            if (filter == null)
                throw new ArgumentNullException(nameof(filter));

            lock (this.Filters)
            {
                var filters = this.Filters;
                if (!filters.Contains(filter))
                    return false;

                return filters.Remove(filter);
            }
        }

        private void ApplyFiltersSync(Memory<byte> pcmSpan)
        {
            var pcm16 = MemoryMarshal.Cast<byte, short>(pcmSpan.Span);

            // pass through any filters, if applicable
            lock (this.Filters)
            {
                if (this.Filters.Any())
                {
                    foreach (var filter in this.Filters)
                        filter.Transform(pcm16, this.Connection.AudioFormat, this.SampleDuration);
                }
            }

            if (this.VolumeModifier != 1)
            {
                // alter volume
                for (var i = 0; i < pcm16.Length; i++)
                    pcm16[i] = (short)(pcm16[i] * this.VolumeModifier);
            }
        }
    }
}
