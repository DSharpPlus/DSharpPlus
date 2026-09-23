using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace DSharpPlus.Voice.MemoryServices;

#pragma warning disable IDE0040

partial class MemoryHelpers
{
    /// <summary>
    /// Expands a section of audio from single-channel (mono) to dual-channel (stereo).
    /// </summary>
    public static void WidenToStereo(ReadOnlySpan<short> mono, Span<Int16x2> stereo)
    {
        ArgumentOutOfRangeException.ThrowIfNotEqual(mono.Length, stereo.Length, nameof(stereo));

        ref byte start = ref MemoryMarshal.GetReference(MemoryMarshal.Cast<short, byte>(mono));
        ref byte targetStart = ref MemoryMarshal.GetReference(MemoryMarshal.Cast<Int16x2, byte>(stereo));
        int index = 0;

        // given the characteristics of how we move memory around, AVX2 lacking a full-lane shuffle makes it not actually any faster
        // than SSSE3 here (and it's quite a bit more annoying to implement), so we only implement a more optimized version for AVX512
        // where we can in fact shuffle (or permute) across the full vector width at the element size we have (AVX2 permute only supports
        // 32-bit and 64-bit elements, we have functionally 16-bit elements)
        if (Avx512Vbmi.IsSupported && Vector512.IsHardwareAccelerated)
        {
            WidenToStereoImpl.V512(ref start, ref targetStart, index, mono.Length);
        }
        else
        {
            WidenToStereoImpl.V128(ref start, ref targetStart, index, mono.Length);
        }
    }
}

file static class WidenToStereoImpl
{
    private static ReadOnlySpan<byte> Avx512VbmiMask =>
    [
          0,  1,  0,  1,  2,  3,  2,  3,  4,  5,  4,  5,  6,  7,  6,  7,
          8,  9,  8,  9, 10, 11, 10, 11, 12, 13, 12, 13, 14, 15, 14, 15,
         32, 33, 32, 33, 34, 35, 34, 35, 36, 37, 36, 37, 38, 39, 38, 39,
         40, 41, 40, 41, 42, 43, 42, 43, 44, 45, 44, 45, 46, 47, 46, 47
    ];

    private static ReadOnlySpan<byte> V128Mask => [0, 1, 8, 9, 2, 3, 10, 11, 4, 5, 12, 13, 6, 7, 14, 15];

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static void V512(ref byte mono, ref byte stereo, int index, int length)
    {
        for (; index <= (length * 2) - 128; index += 128)
        {
            Vector256<byte> i0 = Vector256.LoadUnsafe(ref mono, (nuint)index);
            Vector256<byte> i1 = Vector256.LoadUnsafe(ref mono, (nuint)index + 32);
            Vector256<byte> i2 = Vector256.LoadUnsafe(ref mono, (nuint)index + 64);
            Vector256<byte> i3 = Vector256.LoadUnsafe(ref mono, (nuint)index + 96);

            Vector512<byte> mask = Vector512.Create(Avx512VbmiMask);

            Vector512<byte> v0 = Vector512.Create(Vector256.Create(i0.GetLower()), Vector256.Create(i0.GetUpper()));
            Vector512<byte> v1 = Vector512.Create(Vector256.Create(i1.GetLower()), Vector256.Create(i1.GetUpper()));
            Vector512<byte> v2 = Vector512.Create(Vector256.Create(i2.GetLower()), Vector256.Create(i2.GetUpper()));
            Vector512<byte> v3 = Vector512.Create(Vector256.Create(i3.GetLower()), Vector256.Create(i3.GetUpper()));

            v0 = Avx512Vbmi.PermuteVar64x8(v0, mask);
            v1 = Avx512Vbmi.PermuteVar64x8(v1, mask);
            v2 = Avx512Vbmi.PermuteVar64x8(v2, mask);
            v3 = Avx512Vbmi.PermuteVar64x8(v3, mask);

            v0.StoreUnsafe(ref stereo, (nuint)index * 2);
            v1.StoreUnsafe(ref stereo, (nuint)(index * 2) + 64);
            v2.StoreUnsafe(ref stereo, (nuint)(index * 2) + 128);
            v3.StoreUnsafe(ref stereo, (nuint)(index * 2) + 192);
        }

        V128(ref mono, ref stereo, index, length);
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static void V128(ref byte mono, ref byte stereo, int index, int length)
    {
        for (; index <= (length * 2) - 32; index += 32)
        {
            ulong _0 = Unsafe.ReadUnaligned<ulong>(ref Unsafe.Add(ref mono, index));
            ulong _1 = Unsafe.ReadUnaligned<ulong>(ref Unsafe.Add(ref mono, index + 8));
            ulong _2 = Unsafe.ReadUnaligned<ulong>(ref Unsafe.Add(ref mono, index + 16));
            ulong _3 = Unsafe.ReadUnaligned<ulong>(ref Unsafe.Add(ref mono, index + 24));

            Vector128<byte> mask = Vector128.Create(V128Mask);

            Vector128<byte> v0 = Vector128.Create(_0).As<ulong, byte>();
            Vector128<byte> v1 = Vector128.Create(_1).As<ulong, byte>();
            Vector128<byte> v2 = Vector128.Create(_2).As<ulong, byte>();
            Vector128<byte> v3 = Vector128.Create(_3).As<ulong, byte>();

            v0 = Vector128.Shuffle(v0, mask);
            v1 = Vector128.Shuffle(v1, mask);
            v2 = Vector128.Shuffle(v2, mask);
            v3 = Vector128.Shuffle(v3, mask);

            v0.StoreUnsafe(ref stereo, (nuint)index * 2);
            v1.StoreUnsafe(ref stereo, (nuint)(index * 2) + 16);
            v2.StoreUnsafe(ref stereo, (nuint)(index * 2) + 32);
            v3.StoreUnsafe(ref stereo, (nuint)(index * 2) + 48);
        }

        // fallback loop for any elements not processed by the SIMD loop(s) above
        for (; index < length; index += 2)
        {
            Int16x2 value = new(Unsafe.Add(ref mono, index));
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref stereo, index * 2), value);
        }
    }
}
