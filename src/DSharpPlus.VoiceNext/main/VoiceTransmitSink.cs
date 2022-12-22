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

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.VoiceNext.Codec;

namespace DSharpPlus.VoiceNext;

/// <summary>
/// Sink used to transmit audio data via <see cref="VoiceNextConnection"/>.
/// </summary>
public sealed class VoiceTransmitSink : IDisposable
{
    /// <summary>
    /// Gets the PCM sample duration for this sink.
    /// </summary>
    public int SampleDuration
        => PcmBufferDuration;

    /// <summary>
    /// Gets the length of the PCM buffer for this sink. 
    /// Written packets should adhere to this size, but the sink will adapt to fit.
    /// </summary>
    public int SampleLength
        => PcmBuffer.Length;

    /// <summary>
    /// Gets or sets the volume modifier for this sink. Changing this will alter the volume of the output. 1.0 is 100%.
    /// </summary>
    public double VolumeModifier
    {
        get => _volume;
        set
        {
            if (value < 0 || value > 2.5)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Volume needs to be between 0% and 250%.");
            }

            _volume = value;
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

    internal VoiceTransmitSink(VoiceNextConnection vnc, int pcmBufferDuration)
    {
        Connection = vnc;
        PcmBufferDuration = pcmBufferDuration;
        PcmBuffer = new byte[vnc.AudioFormat.CalculateSampleSize(pcmBufferDuration)];
        PcmMemory = PcmBuffer.AsMemory();
        PcmBufferLength = 0;
        WriteSemaphore = new SemaphoreSlim(1, 1);
        Filters = new List<IVoiceFilter>();
    }

    /// <summary>
    /// Writes PCM data to the sink. The data is prepared for transmission, and enqueued.
    /// </summary>
    /// <param name="buffer">PCM data buffer to send.</param>
    /// <param name="offset">Start of the data in the buffer.</param>
    /// <param name="count">Number of bytes from the buffer.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    public async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default) => await WriteAsync(new ReadOnlyMemory<byte>(buffer, offset, count), cancellationToken).ConfigureAwait(false);

    /// <summary>
    /// Writes PCM data to the sink. The data is prepared for transmission, and enqueued.
    /// </summary>
    /// <param name="buffer">PCM data buffer to send.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    public async Task WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        await WriteSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            int remaining = buffer.Length;
            ReadOnlyMemory<byte> buffSpan = buffer;
            Memory<byte> pcmSpan = PcmMemory;

            while (remaining > 0)
            {
                int len = Math.Min(pcmSpan.Length - PcmBufferLength, remaining);

                Memory<byte> tgt = pcmSpan.Slice(PcmBufferLength);
                ReadOnlyMemory<byte> src = buffSpan.Slice(0, len);

                src.CopyTo(tgt);
                PcmBufferLength += len;
                remaining -= len;
                buffSpan = buffSpan.Slice(len);

                if (PcmBufferLength == PcmBuffer.Length)
                {
                    ApplyFiltersSync(pcmSpan);

                    PcmBufferLength = 0;

                    byte[] packet = ArrayPool<byte>.Shared.Rent(PcmMemory.Length);
                    Memory<byte> packetMemory = packet.AsMemory().Slice(0, PcmMemory.Length);
                    PcmMemory.CopyTo(packetMemory);

                    await Connection.EnqueuePacketAsync(new RawVoicePacket(packetMemory, PcmBufferDuration, false, packet), cancellationToken).ConfigureAwait(false);
                }
            }
        }
        finally
        {
            WriteSemaphore.Release();
        }
    }

    /// <summary>
    /// Flushes the rest of the PCM data in this buffer to VoiceNext packet queue.
    /// </summary>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    public async Task FlushAsync(CancellationToken cancellationToken = default)
    {
        Memory<byte> pcm = PcmMemory;
        Helpers.ZeroFill(pcm.Slice(PcmBufferLength).Span);

        ApplyFiltersSync(pcm);

        byte[] packet = ArrayPool<byte>.Shared.Rent(pcm.Length);
        Memory<byte> packetMemory = packet.AsMemory().Slice(0, pcm.Length);
        pcm.CopyTo(packetMemory);

        await Connection.EnqueuePacketAsync(new RawVoicePacket(packetMemory, PcmBufferDuration, false, packet), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Pauses playback.
    /// </summary>
    public void Pause()
        => Connection.Pause();

    /// <summary>
    /// Resumes playback.
    /// </summary>
    /// <returns></returns>
    public async Task ResumeAsync()
        => await Connection.ResumeAsync().ConfigureAwait(false);

    /// <summary>
    /// Gets the collection of installed PCM filters, in order of their execution.
    /// </summary>
    /// <returns>Installed PCM filters, in order of execution.</returns>
    public IEnumerable<IVoiceFilter> GetInstalledFilters()
    {
        foreach (IVoiceFilter filter in Filters)
        {
            yield return filter;
        }
    }

    /// <summary>
    /// Installs a new PCM filter, with specified execution order.
    /// </summary>
    /// <param name="filter">Filter to install.</param>
    /// <param name="order">Order of the new filter. This determines where the filter will be inserted in the filter pipeline.</param>
    public void InstallFilter(IVoiceFilter filter, int order = int.MaxValue)
    {
        if (filter == null)
        {
            throw new ArgumentNullException(nameof(filter));
        }

        if (order < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(order), "Filter order must be greater than or equal to 0.");
        }

        lock (Filters)
        {
            List<IVoiceFilter> filters = Filters;
            if (order >= filters.Count)
            {
                filters.Add(filter);
            }
            else
            {
                filters.Insert(order, filter);
            }
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
        {
            throw new ArgumentNullException(nameof(filter));
        }

        lock (Filters)
        {
            List<IVoiceFilter> filters = Filters;
            return !filters.Contains(filter) ? false : filters.Remove(filter);
        }
    }

    private void ApplyFiltersSync(Memory<byte> pcmSpan)
    {
        Span<short> pcm16 = MemoryMarshal.Cast<byte, short>(pcmSpan.Span);

        // pass through any filters, if applicable
        lock (Filters)
        {
            if (Filters.Any())
            {
                foreach (IVoiceFilter filter in Filters)
                {
                    filter.Transform(pcm16, Connection.AudioFormat, SampleDuration);
                }
            }
        }

        if (VolumeModifier != 1)
        {
            // alter volume
            for (int i = 0; i < pcm16.Length; i++)
            {
                pcm16[i] = (short)(pcm16[i] * VolumeModifier);
            }
        }
    }

    public void Dispose()
        => WriteSemaphore?.Dispose();
}
