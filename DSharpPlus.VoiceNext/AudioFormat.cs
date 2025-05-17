using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace DSharpPlus.VoiceNext;

/// <summary>
/// Defines the format of PCM data consumed or produced by Opus.
/// </summary>
public readonly struct AudioFormat
{
    /// <summary>
    /// Gets the collection of sampling rates (in Hz) the Opus encoder can use.
    /// </summary>
    public static IReadOnlyCollection<int> AllowedSampleRates { get; } = new ReadOnlyCollection<int>(new[] { 8000, 12000, 16000, 24000, 48000 });

    /// <summary>
    /// Gets the collection of channel counts the Opus encoder can use.
    /// </summary>
    public static IReadOnlyCollection<int> AllowedChannelCounts { get; } = new ReadOnlyCollection<int>(new[] { 1, 2 });

    /// <summary>
    /// Gets the collection of sample durations (in ms) the Opus encoder can use.
    /// </summary>
    public static IReadOnlyCollection<int> AllowedSampleDurations { get; } = new ReadOnlyCollection<int>(new[] { 5, 10, 20, 40, 60 });

    /// <summary>
    /// Gets the default audio format. This is a format configured for 48kHz sampling rate, 2 channels, with music quality preset.
    /// </summary>
    public static AudioFormat Default { get; } = new AudioFormat(48000, 2, VoiceApplication.Music);

    /// <summary>
    /// Gets the audio sampling rate in Hz.
    /// </summary>
    public int SampleRate { get; }

    /// <summary>
    /// Gets the audio channel count.
    /// </summary>
    public int ChannelCount { get; }

    /// <summary>
    /// Gets the voice application, which dictates the quality preset.
    /// </summary>
    public VoiceApplication VoiceApplication { get; }

    /// <summary>
    /// Creates a new audio format for use with Opus encoder.
    /// </summary>
    /// <param name="sampleRate">Audio sampling rate in Hz.</param>
    /// <param name="channelCount">Number of audio channels in the data.</param>
    /// <param name="voiceApplication">Encoder preset to use.</param>
    public AudioFormat(int sampleRate = 48000, int channelCount = 2, VoiceApplication voiceApplication = VoiceApplication.Music)
    {
        if (!AllowedSampleRates.Contains(sampleRate))
        {
            throw new ArgumentOutOfRangeException(nameof(sampleRate), "Invalid sample rate specified.");
        }

        if (!AllowedChannelCounts.Contains(channelCount))
        {
            throw new ArgumentOutOfRangeException(nameof(channelCount), "Invalid channel count specified.");
        }

        if (voiceApplication is not VoiceApplication.Music and not VoiceApplication.Voice and not VoiceApplication.LowLatency)
        {
            throw new ArgumentOutOfRangeException(nameof(voiceApplication), "Invalid voice application specified.");
        }

        this.SampleRate = sampleRate;
        this.ChannelCount = channelCount;
        this.VoiceApplication = voiceApplication;
    }

    /// <summary>
    /// Calculates a sample size in bytes.
    /// </summary>
    /// <param name="sampleDuration">Millisecond duration of a sample.</param>
    /// <returns>Calculated sample size in bytes.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly int CalculateSampleSize(int sampleDuration)
    {
        if (!AllowedSampleDurations.Contains(sampleDuration))
        {
            throw new ArgumentOutOfRangeException(nameof(sampleDuration), "Invalid sample duration specified.");
        }

        // Sample size in bytes is a product of the following:
        // - duration in milliseconds
        // - number of channels
        // - sample rate in kHz
        // - size of data (in this case, sizeof(int16_t))
        // which comes down to below:
        return sampleDuration * this.ChannelCount * (this.SampleRate / 1000) * 2;
    }

    /// <summary>
    /// Gets the maximum buffer size for decoding. This method should be called when decoding Opus data to PCM, to ensure sufficient buffer size.
    /// </summary>
    /// <returns>Buffer size required to decode data.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetMaximumBufferSize()
        => CalculateMaximumFrameSize();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal readonly int CalculateSampleDuration(int sampleSize)
        => sampleSize / (this.SampleRate / 1000) / this.ChannelCount / 2 /* sizeof(int16_t) */;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal readonly int CalculateFrameSize(int sampleDuration)
        => sampleDuration * (this.SampleRate / 1000);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal readonly int CalculateMaximumFrameSize()
        => 120 * (this.SampleRate / 1000);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal readonly int SampleCountToSampleSize(int sampleCount)
        => sampleCount * this.ChannelCount * 2 /* sizeof(int16_t) */;

    internal readonly bool IsValid()
        => AllowedSampleRates.Contains(this.SampleRate) && AllowedChannelCounts.Contains(this.ChannelCount) &&
            (this.VoiceApplication == VoiceApplication.Music || this.VoiceApplication == VoiceApplication.Voice || this.VoiceApplication == VoiceApplication.LowLatency);
}
