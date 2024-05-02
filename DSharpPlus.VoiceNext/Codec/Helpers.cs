
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DSharpPlus.VoiceNext.Codec;
internal static class Helpers
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ZeroFill(Span<byte> buff)
    {
        int zero = 0;
        int i = 0;
        for (; i < buff.Length / 4; i++)
        {
            MemoryMarshal.Write(buff, zero);
        }

        int remainder = buff.Length % 4;
        if (remainder == 0)
        {
            return;
        }

        for (; i < buff.Length; i++)
        {
            buff[i] = 0;
        }
    }
}
