using System;
using System.Buffers;

namespace DSharpPlus.Voice.Codec;

/// <summary>
/// Provides a mechanism to decode audio from other users.
/// </summary>
public interface IAudioDecoder : IDisposable
{
    /// <summary>
    /// Decodes the provided packet into the provided writer.
    /// </summary>
    public void Decode(ReadOnlySpan<byte> packet, IBufferWriter<short> writer);
}
