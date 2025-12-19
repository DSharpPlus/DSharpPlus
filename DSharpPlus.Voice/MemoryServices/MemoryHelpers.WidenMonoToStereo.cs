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
    public static unsafe void WidenToStereo(ReadOnlySpan<short> mono, Span<Int16x2> stereo)
    {
        ArgumentOutOfRangeException.ThrowIfNotEqual(mono.Length, stereo.Length, nameof(stereo));

        ref byte start = ref MemoryMarshal.GetReference(MemoryMarshal.Cast<short, byte>(mono));
        ref byte targetStart = ref MemoryMarshal.GetReference(MemoryMarshal.Cast<Int16x2, byte>(stereo));
        int index = 0;

        // 256KB threshold, same as employed by the runtime
        // we pin both spans because we're going to be manually aligning our operations, and that's just easier to do here
        // it would of course be terrible if the GC decided to move our buffers with its guaranteed 8-byte alignment, considering
        // we need to align to 64 bytes.
        if (Avx512Vbmi.IsSupported)
        {
            fixed (short* pIn = mono)
            fixed (Int16x2* pOut = stereo)
            {
                if (mono.Length >= 131072 && CanAlign(pIn, pOut))
                {
                    WidenToStereoImpl.Avx512VbmiGreaterThan256KB(ref start, ref targetStart, index, mono.Length);
                }
                else
                {
                    WidenToStereoImpl.Avx512VbmiLessThan256KB(ref start, ref targetStart, index, mono.Length);
                }
            }
        }
        else
        {
            WidenToStereoImpl.V128(ref start, ref targetStart, index, mono.Length);
        }

        // if we're given unmanaged memory that isn't aligned to the native alignment of ints and shorts respectively, we can't fix it up
        static bool CanAlign(void* pIn, void* pOut)
        {
            return (nuint)pIn % 2 == 0 && (nuint)pOut % 4 == 0;
        }
    }
}

file static unsafe class WidenToStereoImpl
{
    private static ReadOnlySpan<byte> Avx512VbmiMask =>
    [
          0,  1,  0,  1,  2,  3,  2,  3,  4,  5,  4,  5,  6,  7,  6,  7,
          8,  9,  8,  9, 10, 11, 10, 11, 12, 13, 12, 13, 14, 15, 14, 15,
         32, 33, 32, 33, 34, 35, 34, 35, 36, 37, 36, 37, 38, 39, 38, 39,
         40, 41, 40, 41, 42, 43, 42, 43, 44, 45, 44, 45, 46, 47, 46, 47
    ];

    private static ReadOnlySpan<byte> V128Mask => [0, 1, 8, 9, 2, 3, 10, 11, 4, 5, 12, 13, 6, 7, 14, 15];

    public static void Avx512VbmiGreaterThan256KB(ref byte mono, ref byte stereo, int index, int length)
    {
        // start by doing one operation so that we guarantee the start is covered
        Vector256<byte> firstLoad = Vector256.LoadUnsafe(ref Unsafe.Add(ref mono, index));
        Vector512<byte> start = Vector512.Create(Vector256.Create(firstLoad.GetLower()), Vector256.Create(firstLoad.GetUpper()));

        Vector512<byte> mask = Vector512.Create(Avx512VbmiMask);

        start = Avx512Vbmi.PermuteVar64x8(start, mask);
        start.StoreUnsafe(ref stereo);

        // trying to align both loads and stores isn't always possible, and since unaligned stores are more expensive,
        // align those

        byte* pTarget = (byte*)Unsafe.AsPointer(ref stereo);

        nuint misalignment = 64 - ((nuint)pTarget % 64);
        pTarget += misalignment;
        index += (int)(misalignment / 2);

        for (; index <= (length * 2) - 128; index += 128)
        {
            Vector256<byte> i0 = Vector256.LoadUnsafe(ref Unsafe.Add(ref mono, index));
            Vector256<byte> i1 = Vector256.LoadUnsafe(ref Unsafe.Add(ref mono, index + 32));
            Vector256<byte> i2 = Vector256.LoadUnsafe(ref Unsafe.Add(ref mono, index + 64));
            Vector256<byte> i3 = Vector256.LoadUnsafe(ref Unsafe.Add(ref mono, index + 96));

            Vector512<byte> v0 = Vector512.Create(Vector256.Create(i0.GetLower()), Vector256.Create(i0.GetUpper()));
            Vector512<byte> v1 = Vector512.Create(Vector256.Create(i1.GetLower()), Vector256.Create(i1.GetUpper()));
            Vector512<byte> v2 = Vector512.Create(Vector256.Create(i2.GetLower()), Vector256.Create(i2.GetUpper()));
            Vector512<byte> v3 = Vector512.Create(Vector256.Create(i3.GetLower()), Vector256.Create(i3.GetUpper()));

            v0 = Avx512Vbmi.PermuteVar64x8(v0, mask);
            v1 = Avx512Vbmi.PermuteVar64x8(v1, mask);
            v2 = Avx512Vbmi.PermuteVar64x8(v2, mask);
            v3 = Avx512Vbmi.PermuteVar64x8(v3, mask);

            v0.StoreAlignedNonTemporal(pTarget);
            v1.StoreAlignedNonTemporal(pTarget + 64);
            v2.StoreAlignedNonTemporal(pTarget + 128);
            v3.StoreAlignedNonTemporal(pTarget + 192);

            pTarget += 256;
        }

        // lastly, clean up the remaining bytes
        V128(ref mono, ref stereo, index, length);
    }

    public static void Avx512VbmiLessThan256KB(ref byte mono, ref byte stereo, int index, int length)
    {
        for (; index <= (length * 2) - 128; index += 128)
        {
            Vector256<byte> i0 = Vector256.LoadUnsafe(ref Unsafe.Add(ref mono, index));
            Vector256<byte> i1 = Vector256.LoadUnsafe(ref Unsafe.Add(ref mono, index + 32));
            Vector256<byte> i2 = Vector256.LoadUnsafe(ref Unsafe.Add(ref mono, index + 64));
            Vector256<byte> i3 = Vector256.LoadUnsafe(ref Unsafe.Add(ref mono, index + 96));

            Vector512<byte> mask = Vector512.Create(Avx512VbmiMask);

            Vector512<byte> v0 = Vector512.Create(Vector256.Create(i0.GetLower()), Vector256.Create(i0.GetUpper()));
            Vector512<byte> v1 = Vector512.Create(Vector256.Create(i1.GetLower()), Vector256.Create(i1.GetUpper()));
            Vector512<byte> v2 = Vector512.Create(Vector256.Create(i2.GetLower()), Vector256.Create(i2.GetUpper()));
            Vector512<byte> v3 = Vector512.Create(Vector256.Create(i3.GetLower()), Vector256.Create(i3.GetUpper()));

            v0 = Avx512Vbmi.PermuteVar64x8(v0, mask);
            v1 = Avx512Vbmi.PermuteVar64x8(v1, mask);
            v2 = Avx512Vbmi.PermuteVar64x8(v2, mask);
            v3 = Avx512Vbmi.PermuteVar64x8(v3, mask);

            v0.StoreUnsafe(ref Unsafe.Add(ref stereo, index * 2));
            v1.StoreUnsafe(ref Unsafe.Add(ref stereo, (index * 2) + 64));
            v2.StoreUnsafe(ref Unsafe.Add(ref stereo, (index * 2) + 128));
            v3.StoreUnsafe(ref Unsafe.Add(ref stereo, (index * 2) + 192));
        }

        V128(ref mono, ref stereo, index, length);
    }

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

            v0.StoreUnsafe(ref Unsafe.Add(ref stereo, index * 2));
            v1.StoreUnsafe(ref Unsafe.Add(ref stereo, (index * 2) + 16));
            v2.StoreUnsafe(ref Unsafe.Add(ref stereo, (index * 2) + 32));
            v3.StoreUnsafe(ref Unsafe.Add(ref stereo, (index * 2) + 48));
        }

        // fallback loop for any elements not processed by the SIMD loop(s) above
        for (; index < length; index += 2)
        {
            Int16x2 value = new(Unsafe.Add(ref mono, index));
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref stereo, index * 2), value);
        }
    }
}
