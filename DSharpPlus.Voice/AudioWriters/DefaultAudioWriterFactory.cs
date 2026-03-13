using System;

using DSharpPlus.Voice.Codec;
using DSharpPlus.Voice.MemoryServices.Channels;

namespace DSharpPlus.Voice.AudioWriters;

/// <summary>
/// The default mechanism for creating audio writers.
/// </summary>
public sealed class DefaultAudioWriterFactory : IAudioWriterFactory
{
    private readonly IAudioCodec codec;

    public DefaultAudioWriterFactory(IAudioCodec codec) 
        => this.codec = codec;

    /// <inheritdoc/>
    public AbstractAudioWriter CreateAudioWriter(AudioFormat format, AudioChannelWriter writer)
    {
        IAudioEncoder encoder = this.codec.GetEncoder();

        return format.Identifier switch
        {
            "s16le-48khz-stereo-pcm" => new S16LE48KHzStereoWriter(encoder, writer),
            "s16le-48khz-mono-pcm" => new S16LE48KHzMonoWriter(encoder, writer),
            "s16le-44khz-stereo-pcm" => new S16LE44KHzStereoWriter(encoder, writer),
            "s16le-44khz-mono-pcm" => new S16LE44KHzMonoWriter(encoder, writer),
            "s24le-48khz-stereo-pcm" => new S24LE48KHzStereoWriter(encoder, writer),
            "float32le-48khz-stereo-pcm" => new Float32LE48KHzStereoWriter(encoder, writer),
            _ => throw new InvalidOperationException($"Unknown audio format {format}.")
        };
    }
}
