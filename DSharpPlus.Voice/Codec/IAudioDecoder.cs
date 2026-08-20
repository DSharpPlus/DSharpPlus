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
    /// <remarks>
    /// When implementing this method, note that calling <see cref="IBufferWriter{T}.GetMemory(int)"/> on <paramref name="writer"/> is illegal
    /// due to the nature of the type and memory provided by DSharpPlus.
    /// </remarks>
    public void Decode(ReadOnlySpan<byte> packet, IBufferWriter<short> writer);
}
