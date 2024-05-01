namespace DSharpPlus.VoiceNext;

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.VoiceNext.Codec;

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
        Filters = [];
    }

    /// <summary>
    /// Writes PCM data to the sink. The data is prepared for transmission, and enqueued.
    /// </summary>
    /// <param name="buffer">PCM data buffer to send.</param>
    /// <param name="offset">Start of the data in the buffer.</param>
    /// <param name="count">Number of bytes from the buffer.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    public async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default) => await WriteAsync(new ReadOnlyMemory<byte>(buffer, offset, count), cancellationToken);

    /// <summary>
    /// Writes PCM data to the sink. The data is prepared for transmission, and enqueued.
    /// </summary>
    /// <param name="buffer">PCM data buffer to send.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    public async Task WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        await WriteSemaphore.WaitAsync(cancellationToken);

        try
        {
            int remaining = buffer.Length;
            ReadOnlyMemory<byte> buffSpan = buffer;
            Memory<byte> pcmSpan = PcmMemory;

            while (remaining > 0)
            {
                int len = Math.Min(pcmSpan.Length - PcmBufferLength, remaining);

                Memory<byte> tgt = pcmSpan[PcmBufferLength..];
                ReadOnlyMemory<byte> src = buffSpan[..len];

                src.CopyTo(tgt);
                PcmBufferLength += len;
                remaining -= len;
                buffSpan = buffSpan[len..];

                if (PcmBufferLength == PcmBuffer.Length)
                {
                    ApplyFiltersSync(pcmSpan);

                    PcmBufferLength = 0;

                    byte[] packet = ArrayPool<byte>.Shared.Rent(PcmMemory.Length);
                    Memory<byte> packetMemory = packet.AsMemory(0, PcmMemory.Length);
                    PcmMemory.CopyTo(packetMemory);

                    await Connection.EnqueuePacketAsync(new RawVoicePacket(packetMemory, PcmBufferDuration, false, packet), cancellationToken);
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
        Helpers.ZeroFill(pcm[PcmBufferLength..].Span);

        ApplyFiltersSync(pcm);

        byte[] packet = ArrayPool<byte>.Shared.Rent(pcm.Length);
        Memory<byte> packetMemory = packet.AsMemory(0, pcm.Length);
        pcm.CopyTo(packetMemory);

        await Connection.EnqueuePacketAsync(new RawVoicePacket(packetMemory, PcmBufferDuration, false, packet), cancellationToken);
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
        => await Connection.ResumeAsync();

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
        ArgumentNullException.ThrowIfNull(filter);
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
        ArgumentNullException.ThrowIfNull(filter);
        lock (Filters)
        {
            List<IVoiceFilter> filters = Filters;
            return filters.Contains(filter) && filters.Remove(filter);
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
