namespace DSharpPlus.Voice.Codec;

/// <summary>
/// Provides a mechanism to obtain audio encoders and decoders.
/// </summary>
public interface IAudioCodec
{
    /// <summary>
    /// Creates a new encoder for a connection.
    /// </summary>
    /// <param name="bitrate">The bitrate permitted for this connection.</param>
    /// <param name="type">The starting audio type for this connection.</param>
    /// <param name="isStreamingConnection">
    /// A hint as to whether audio is more likely to be streamed to this connection or submitted in bulk to this connection.
    /// </param>
    public IAudioEncoder CreateEncoder(int bitrate, AudioType type, bool isStreamingConnection);

    /// <summary>
    /// Creates a new decoder for a connection.
    /// </summary>
    public IAudioDecoder CreateDecoder();
}
