using System;
using System.Buffers;
using System.Collections.Generic;
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
        => this.PcmBufferDuration;

    /// <summary>
    /// Gets the length of the PCM buffer for this sink.
    /// Written packets should adhere to this size, but the sink will adapt to fit.
    /// </summary>
    public int SampleLength
        => this.PcmBuffer.Length;

    /// <summary>
    /// Gets or sets the volume modifier for this sink. Changing this will alter the volume of the output. 1.0 is 100%.
    /// </summary>
    public double VolumeModifier
    {
        get => this.volume;
        set
        {
            if (value is < 0 or > 2.5)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Volume needs to be between 0% and 250%.");
            }

            this.volume = value;
        }
    }
    private double volume = 1.0;

    private VoiceNextConnection Connection { get; }
    private int PcmBufferDuration { get; }
    private byte[] PcmBuffer { get; }
    private Memory<byte> PcmMemory { get; }
    private int PcmBufferLength { get; set; }
    private SemaphoreSlim WriteSemaphore { get; }
    private List<IVoiceFilter> Filters { get; }

    internal VoiceTransmitSink(VoiceNextConnection vnc, int pcmBufferDuration)
    {
        this.Connection = vnc;
        this.PcmBufferDuration = pcmBufferDuration;
        this.PcmBuffer = new byte[vnc.AudioFormat.CalculateSampleSize(pcmBufferDuration)];
        this.PcmMemory = this.PcmBuffer.AsMemory();
        this.PcmBufferLength = 0;
        this.WriteSemaphore = new SemaphoreSlim(1, 1);
        this.Filters = [];
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
        await this.WriteSemaphore.WaitAsync(cancellationToken);

        try
        {
            int remaining = buffer.Length;
            ReadOnlyMemory<byte> buffSpan = buffer;
            Memory<byte> pcmSpan = this.PcmMemory;

            while (remaining > 0)
            {
                int len = Math.Min(pcmSpan.Length - this.PcmBufferLength, remaining);

                Memory<byte> tgt = pcmSpan[this.PcmBufferLength..];
                ReadOnlyMemory<byte> src = buffSpan[..len];

                src.CopyTo(tgt);
                this.PcmBufferLength += len;
                remaining -= len;
                buffSpan = buffSpan[len..];

                if (this.PcmBufferLength == this.PcmBuffer.Length)
                {
                    ApplyFiltersSync(pcmSpan);

                    this.PcmBufferLength = 0;

                    byte[] packet = ArrayPool<byte>.Shared.Rent(this.PcmMemory.Length);
                    Memory<byte> packetMemory = packet.AsMemory(0, this.PcmMemory.Length);
                    this.PcmMemory.CopyTo(packetMemory);

                    await this.Connection.EnqueuePacketAsync(new RawVoicePacket(packetMemory, this.PcmBufferDuration, false, packet), cancellationToken);
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
        Memory<byte> pcm = this.PcmMemory;
        Helpers.ZeroFill(pcm[this.PcmBufferLength..].Span);

        ApplyFiltersSync(pcm);

        byte[] packet = ArrayPool<byte>.Shared.Rent(pcm.Length);
        Memory<byte> packetMemory = packet.AsMemory(0, pcm.Length);
        pcm.CopyTo(packetMemory);

        await this.Connection.EnqueuePacketAsync(new RawVoicePacket(packetMemory, this.PcmBufferDuration, false, packet), cancellationToken);
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
        => await this.Connection.ResumeAsync();

    /// <summary>
    /// Gets the collection of installed PCM filters, in order of their execution.
    /// </summary>
    /// <returns>Installed PCM filters, in order of execution.</returns>
    public IEnumerable<IVoiceFilter> GetInstalledFilters()
    {
        foreach (IVoiceFilter filter in this.Filters)
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

        lock (this.Filters)
        {
            List<IVoiceFilter> filters = this.Filters;
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
        lock (this.Filters)
        {
            List<IVoiceFilter> filters = this.Filters;
            return filters.Contains(filter) && filters.Remove(filter);
        }
    }

    private void ApplyFiltersSync(Memory<byte> pcmSpan)
    {
        Span<short> pcm16 = MemoryMarshal.Cast<byte, short>(pcmSpan.Span);

        // pass through any filters, if applicable
        lock (this.Filters)
        {
            if (this.Filters.Count != 0)
            {
                foreach (IVoiceFilter filter in this.Filters)
                {
                    filter.Transform(pcm16, this.Connection.AudioFormat, this.SampleDuration);
                }
            }
        }

        if (this.VolumeModifier != 1)
        {
            // alter volume
            for (int i = 0; i < pcm16.Length; i++)
            {
                pcm16[i] = (short)(pcm16[i] * this.VolumeModifier);
            }
        }
    }

    public void Dispose()
        => this.WriteSemaphore?.Dispose();
}
