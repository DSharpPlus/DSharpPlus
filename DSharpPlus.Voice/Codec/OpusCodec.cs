using DSharpPlus.Voice.RuntimeServices.Memory;

namespace DSharpPlus.Voice.Codec;

public sealed class OpusCodec : IAudioCodec
{
    /// <inheritdoc/>
    public (IAudioEncoder encoder, IAudioDecoder decoder) CreateEncoderDecoderPair(int bitrate, AudioType type, bool isStreamingConnection)
    {
        AudioBufferPool pool = isStreamingConnection
            ? new TinyAudioBufferPool(type == AudioType.Voice ? 972 : 5772)
            : type == AudioType.Voice ? AudioBufferPool.Opus20ms : AudioBufferPool.Opus120ms;

        IAudioEncoder encoder = new OpusEncoder(pool, bitrate, type);
        IAudioDecoder decoder = new OpusDecoder();

        return (encoder, decoder);
    }
}
