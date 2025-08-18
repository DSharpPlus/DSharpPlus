namespace DSharpPlus.Voice.Codec;

/// <summary>
/// Provides a mechanism to obtain a matching encoder and decoder pair.
/// </summary>
public interface IAudioCodec
{
    /// <summary>
    /// Creates a new matching encoder-decoder pair for a connection.
    /// </summary>
    /// <param name="bitrate">The bitrate permitted for this connection.</param>
    /// <param name="ssrc">The SSRC of the sender.</param>
    /// <param name="type">The starting audio type for this connection.</param>
    /// <param name="isStreamingConnection">
    /// A hint as to whether audio is more likely to be streamed to this connection or submitted in bulk to this connection.
    /// </param>
    public (IAudioEncoder encoder, IAudioDecoder decoder) CreateEncoderDecoderPair(int bitrate, uint ssrc, AudioType type, bool isStreamingConnection);
}
