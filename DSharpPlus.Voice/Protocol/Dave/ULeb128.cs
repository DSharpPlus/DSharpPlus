using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace DSharpPlus.Voice.Protocol.Dave;

/// <summary>
/// Provides an interface to encode and decode ULEB128 integers.
/// </summary>
[SkipLocalsInit]
internal static unsafe class ULeb128
{
    /// <summary>
    /// Encodes a given 32-bit integer as ULEB128 to the given span. Up to five bytes may be consumed.
    /// </summary>
    /// <param name="destination">A span of bytes to encode to. This must be long enough to contain the entire integer.</param>
    /// <param name="value">The 32-bit integer value to encode. This must not be negative.</param>
    /// <param name="written">The count of bytes written to the span.</param>
    /// <returns>A value indicating success. If writing was unsuccessful, no bytes will be written.</returns>
    public static bool TryWriteInt32(scoped Span<byte> destination, int value, out int written)
    {
        if (value >= 0)
        {
            return TryWriteUInt32(destination, (uint)value, out written);
        }

        written = 0;
        return false;
    }

    /// <summary>
    /// Encodes a given 32-bit integer as ULEB128 to the given span. Up to five bytes may be consumed.
    /// </summary>
    /// <param name="destination">A span of bytes to encode to. This must be long enough to contain the entire integer.</param>
    /// <param name="value">The 32-bit integer value to encode.</param>
    /// <param name="written">The count of bytes written to the span.</param>
    /// <returns>A value indicating success. If writing was unsuccessful, no bytes will be written.</returns>
    public static bool TryWriteUInt32(scoped Span<byte> destination, uint value, out int written)
    {
        // success path first to make the branch predictor happy, no matter how goofy this looks
        // the branch predictor already really hates this code, anyway - i feel you on that, branch predictor
        if (destination.Length >= GetEncodedUInt32Length(value))
        {
            fixed (byte* dst = destination)
            {
                byte* p = dst;

                do
                {
                    byte b = (byte)(value & 0x7F);
                    value >>= 7;

                    b |= 0x80;

                    *p++ = b;
                } while (value != 0);

                *(p - 1) &= 0x7F;

                written = (int)(p - dst);
            }

            return true;
        }
        else
        {
            written = 0;
            return false;
        }
    }

    /// <summary>
    /// Encodes a given 64-bit integer as ULEB128 to the given span. Up to 10 bytes may be consumed.
    /// </summary>
    /// <param name="destination">A span of bytes to encode to. This must be long enough to contain the entire integer.</param>
    /// <param name="value">The 64-bit integer value to encode. This must not be negative.</param>
    /// <param name="written">The count of bytes written to the span.</param>
    /// <returns>A value indicating success. If writing was unsuccessful, no bytes will be written.</returns>
    public static bool TryWriteInt64(scoped Span<byte> destination, long value, out int written)
    {
        if (value >= 0)
        {
            return TryWriteUInt64(destination, (ulong)value, out written);
        }

        written = 0;
        return false;
    }

    /// <summary>
    /// Encodes a given 64-bit integer as ULEB128 to the given span. Up to 10 bytes may be consumed.
    /// </summary>
    /// <param name="destination">A span of bytes to encode to. This must be long enough to contain the entire integer.</param>
    /// <param name="value">The 64-bit integer value to encode.</param>
    /// <param name="written">The count of bytes written to the span.</param>
    /// <returns>A value indicating success. If writing was unsuccessful, no bytes will be written.</returns>
    public static bool TryWriteUInt64(scoped Span<byte> destination, ulong value, out int written)
    {
        if (destination.Length >= GetEncodedUInt64Length(value))
        {
            fixed (byte* dst = destination)
            {
                byte* p = dst;

                do
                {
                    byte b = (byte)(value & 0x7F);
                    value >>= 7;

                    b |= 0x80;

                    *p++ = b;
                } while (value != 0);

                *(p - 1) &= 0x7F;

                written = (int)(p - dst);
            }

            return true;
        }
        else
        {
            written = 0;
            return false;
        }
    }

    /// <summary>
    /// Decodes a 32-bit integer from the given span.
    /// </summary>
    /// <param name="source">The raw ULEB128-encoded data to decode from.</param>
    /// <param name="value">The decoded value.</param>
    /// <param name="read">The amount of bytes read from the source span.</param>
    /// <returns>A value indicating success. If writing was unsuccessful, no bytes will be read.</returns>
    public static bool TryReadInt32(scoped ReadOnlySpan<byte> source, out int value, out int read)
    {
        if (!TryReadUInt32(source, out uint result, out int tempRead) || result > int.MaxValue)
        {
            value = 0;
            read = 0;
            return false;
        }

        value = (int)result;
        read = tempRead;
        return true;
    }

    /// <summary>
    /// Decodes a 32-bit integer from the given span.
    /// </summary>
    /// <param name="source">The raw ULEB128-encoded data to decode from.</param>
    /// <param name="value">The decoded value.</param>
    /// <param name="read">The amount of bytes read from the source span.</param>
    /// <returns>A value indicating success. If writing was unsuccessful, no bytes will be read.</returns>
    public static bool TryReadUInt32(scoped ReadOnlySpan<byte> source, out uint value, out int read)
    {
        if (IsValidULeb128Span(source))
        {
            value = 0;

            fixed (byte* src = source)
            {
                byte* p = src;
                int shift = 0;

                do
                {
                    int slice = *p & 0x7F;
                    value += (uint)(slice << shift);
                    shift += 7;
                } while (*p++ >= 128);

                if ((read = (int)(p - src)) > 5)
                {
                    value = 0;
                    read = 0;
                    return false;
                }
            }

            return true;
        }
        else
        {
            value = 0;
            read = 0;
            return false;
        }
    }

    /// <summary>
    /// Decodes a 64-bit integer from the given span.
    /// </summary>
    /// <param name="source">The raw ULEB128-encoded data to decode from.</param>
    /// <param name="value">The decoded value.</param>
    /// <param name="read">The amount of bytes read from the source span.</param>
    /// <returns>A value indicating success. If writing was unsuccessful, no bytes will be read.</returns>
    public static bool TryReadInt64(scoped ReadOnlySpan<byte> source, out long value, out int read)
    {
        if (!TryReadUInt64(source, out ulong result, out int tempRead) || result > long.MaxValue)
        {
            value = 0;
            read = 0;
            return false;
        }

        value = (long)result;
        read = tempRead;
        return true;
    }

    /// <summary>
    /// Decodes a 64-bit integer from the given span.
    /// </summary>
    /// <param name="source">The raw ULEB128-encoded data to decode from.</param>
    /// <param name="value">The decoded value.</param>
    /// <param name="read">The amount of bytes read from the source span.</param>
    /// <returns>A value indicating success. If writing was unsuccessful, no bytes will be read.</returns>
    public static bool TryReadUInt64(scoped ReadOnlySpan<byte> source, out ulong value, out int read)
    {
        if (IsValidULeb128Span(source))
        {
            value = 0;

            fixed (byte* src = source)
            {
                byte* p = src;
                int shift = 0;

                do
                {
                    int slice = *p & 0x7F;
                    value += (uint)(slice << shift);
                    shift += 7;
                } while (*p++ >= 128);

                if ((read = (int)(p - src)) > 10)
                {
                    value = 0;
                    read = 0;
                    return false;
                }
            }

            return true;
        }
        else
        {
            value = 0;
            read = 0;
            return false;
        }
    }

    private static int GetEncodedUInt32Length(uint value)
        => ((31 - BitOperations.LeadingZeroCount(value)) / 7) + 1;

    private static int GetEncodedUInt64Length(ulong value)
        => ((63 - BitOperations.LeadingZeroCount(value)) / 7) + 1;

    private static bool IsValidULeb128Span(scoped ReadOnlySpan<byte> span)
    {
        int i = 0;

        for (; i + 3 < span.Length; i += 4)
        {
            // blocks of 4; we don't need to scan more than 10 bytes at any point - no point in trying to load a (16b) vector
            int value = Unsafe.ReadUnaligned<int>(in span[i]);

            // we want to see whether the first bit is zero anywhere
            if ((~value & 0x80808080) != 0)
            {
                return true;
            }
        }

        for (; i < span.Length; i++)
        {
            if ((span[i] & 0x80) == 0)
            {
                return true;
            }
        }

        return false;
    }
}
