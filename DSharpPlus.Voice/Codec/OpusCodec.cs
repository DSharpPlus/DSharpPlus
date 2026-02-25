using DSharpPlus.Voice.MemoryServices;

namespace DSharpPlus.Voice.Codec;

/// <inheritdoc cref="IAudioCodec"/>
public sealed class OpusCodec : IAudioCodec
{
    /// <inheritdoc/>
    public IAudioEncoder CreateEncoder(int bitrate, AudioType type, bool isStreamingConnection)
    {
        AudioBufferPool pool = isStreamingConnection
            ? new TinyAudioBufferPool(type == AudioType.Voice ? 972 : 5772)
            : type == AudioType.Voice ? AudioBufferPool.Opus20ms : AudioBufferPool.Opus120ms;

        IAudioEncoder encoder = new OpusEncoder(pool, bitrate, type);

        return encoder;
    }

    /// <inheritdoc/>
    public IAudioDecoder CreateDecoder()
        => new OpusDecoder();
}
