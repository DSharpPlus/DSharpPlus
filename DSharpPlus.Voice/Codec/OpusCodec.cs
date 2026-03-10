using System;

namespace DSharpPlus.Voice.Codec;

/// <inheritdoc cref="IAudioCodec"/>
public sealed class OpusCodec : IAudioCodec
{
    private IAudioEncoder? encoder;

    /// <inheritdoc/>
    public IAudioEncoder CreateEncoder(int bitrate, AudioType type)
    {
        IAudioEncoder encoder = new OpusEncoder(bitrate, type);

        this.encoder = encoder;
        return encoder;
    }

    /// <inheritdoc/>
    public IAudioEncoder GetEncoder()
    {
        if (this.encoder is null)
        {
            throw new InvalidOperationException("This may only be called after a connection was established.");
        }

        return this.encoder;
    }

    /// <inheritdoc/>
    public IAudioDecoder CreateDecoder()
        => new OpusDecoder();
}
