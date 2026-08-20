using System;
using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;

namespace DSharpPlus.Voice.AudioWriters.Ogg;

/// <summary>
/// Specifies the contents of an identification header.
/// </summary>
internal sealed record OggOpusIdentificationHeader
{
    /// <summary>
    /// The version of the ogg/opus stream. This is always 1.
    /// </summary>
    public byte Version { get; private init; }

    /// <summary>
    /// The amount of channels this audio was encoded for.
    /// </summary>
    public byte ChannelCount { get; private init; }

    /// <summary>
    /// The amount of samples to skip when starting playback. This is defined to always be at 48KHz, irrespective of
    /// what <see cref="InputSampleRate"/> says. 
    /// </summary>
    public ushort PreSkipSamples { get; private init; }

    /// <summary>
    /// The sample rate of the input audio.
    /// </summary>
    public uint InputSampleRate { get; private init; }

    /// <summary>
    /// A Q7.8 decibel modifier to apply to the audio data during playback. We ignore this and log a warning.
    /// </summary>
    public ushort OutputGain { get; private init; }

    /// <summary>
    /// The family of channel mappings at work in this file. DSharpPlus only supports <see cref="OggOpusChannelMappingFamily.Basic"/>. 
    /// </summary>
    public OggOpusChannelMappingFamily MappingFamily { get; private init; }

    /// <summary>
    /// Attempts to parse an identification header.
    /// </summary>
    public static bool TryParse
    (
        ReadOnlySpan<byte> idHeader, 
        
        [NotNullWhen(true)] 
        out OggOpusIdentificationHeader? info
    )
    {
        info = null;

        if (!IsCompleteIdentificationHeader(idHeader))
        {
            return false;
        }

        byte version = idHeader[8];
        byte channelCount = idHeader[9];
        ushort preSkip = BinaryPrimitives.ReadUInt16LittleEndian(idHeader[10..]);
        uint inputSampleRate = BinaryPrimitives.ReadUInt32LittleEndian(idHeader[12..]);
        ushort outputGain = BinaryPrimitives.ReadUInt16LittleEndian(idHeader[16..]);
        OggOpusChannelMappingFamily mappingFamily = (OggOpusChannelMappingFamily)idHeader[18];

        info = new()
        {
            Version = version,
            ChannelCount = channelCount,
            PreSkipSamples = preSkip,
            InputSampleRate = inputSampleRate,
            OutputGain = outputGain,
            MappingFamily = mappingFamily
        };

        return true;
    }

    public static bool IsCompleteIdentificationHeader(ReadOnlySpan<byte> idHeader)
        => idHeader.StartsWith("OpusHead"u8) && idHeader.Length >= 19 && idHeader[8] == 1;
}
