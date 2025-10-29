using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace DSharpPlus.Voice.MemoryServices;

#pragma warning disable IDE0040 // Add accessibility modifiers

partial class MemoryHelpers
{
    public static unsafe void NarrowToS16LE(ReadOnlySpan<int> s24le, Span<short> s16le)
    {
        ArgumentOutOfRangeException.ThrowIfNotEqual(s24le.Length, s16le.Length, nameof(s16le));

        ref byte start = ref MemoryMarshal.GetReference(MemoryMarshal.Cast<int, byte>(s24le));
        ref byte targetStart = ref MemoryMarshal.GetReference(MemoryMarshal.Cast<short, byte>(s16le));
        int index = 0;

        // 256KB threshold, same as employed by the runtime
        // we pin both spans because we're going to be manually aligning our operations, and that's just easier to do here
        // it would of course be terrible if the GC decided to move our buffers with its guaranteed 8-byte alignment, considering
        // we need to align to 64 bytes.
        if (Avx512Vbmi.IsSupported)
        {
            fixed (int* pIn = s24le)
            fixed (short* pOut = s16le)
            {
                if (s24le.Length >= 65536 && CanAlign(pIn, pOut))
                {
                    NarrowToS16LEImpl.Avx512VbmiGreaterThan256KB(ref start, ref targetStart, index, s24le.Length);
                }
                else
                {
                    NarrowToS16LEImpl.Avx512VbmiLessThan256KB(ref start, ref targetStart, index, s24le.Length);
                }
            }
        }
        else if (Avx2.IsSupported)
        {
            NarrowToS16LEImpl.Avx2(ref start, ref targetStart, index, s24le.Length);
        }
        else
        {
            NarrowToS16LEImpl.V128(ref start, ref targetStart, index, s24le.Length);
        }

        // if we're given unmanaged memory that isn't aligned to the native alignment of ints and shorts respectively, we can't fix it up
        static bool CanAlign(int* pIn, short* pOut)
        {
            return (nuint)pIn % 4 == 0 && (nuint)pOut % 2 == 0;
        }
    }
}

file static unsafe class NarrowToS16LEImpl
{
    private static ReadOnlySpan<byte> Avx512VbmiMask =>
    [
         2,   3,   6,   7,  10,  11,  14,  15,  18,  19,  22,  23,  26,  27,  30,  31,
        34,  35,  38,  39,  42,  43,  46,  47,  50,  51,  54,  55,  58,  59,  62,  63,
        66,  67,  70,  71,  74,  75,  78,  79,  82,  83,  86,  87,  90,  91,  94,  95,
        98,  99, 102, 103, 106, 107, 110, 111, 114, 115, 118, 119, 122, 123, 126, 127
    ];

    private static ReadOnlySpan<byte> Avx2Mask =>
    [
         2,  3,  6,  7, 10, 11, 14, 15,  0,  1,  4,  5,  8,  9, 12, 13,
        18, 19, 22, 23, 26, 27, 30, 31, 16, 17, 20, 21, 24, 25, 28, 29
    ];

    // four packed indices: 0, 2, 1, 3, the order in which we want to permute after the above-specified lane-wise shuffle
    const byte Avx2PermuteMask = 0b00_10_01_11;

    private static ReadOnlySpan<byte> V128Mask => [2, 3, 6, 7, 10, 11, 14, 15, 0, 1, 4, 5, 8, 9, 12, 13];

    public static void Avx512VbmiGreaterThan256KB(ref byte s24le, ref byte s16le, int index, int length)
    {
        // start by doing one operation so that we guarantee the start is covered
        Vector512<byte> start = Vector512.LoadUnsafe(ref s24le);
        Vector512<byte> start2 = Vector512.LoadUnsafe(ref Unsafe.Add(ref s24le, 64));

        Vector512<int> rounding = Vector512.Create(0b00000000_00000000_01111111_11111111);

        start = Vector512.Add(start.AsInt32(), rounding).AsByte();
        start2 = Vector512.Add(start2.AsInt32(), rounding).AsByte();

        Vector512<byte> mask = Vector512.Create(Avx512VbmiMask);

        start = Avx512Vbmi.PermuteVar64x8x2(start, mask, start2);
        start.StoreUnsafe(ref s16le);

        // trying to align both loads and stores isn't always possible, and since unaligned stores are more expensive,
        // align those

        byte* pTarget = (byte*)Unsafe.AsPointer(ref s16le);

        nuint misalignment = 64 - ((nuint)pTarget % 64);
        pTarget += misalignment;
        index += (int)(2 * misalignment);

        // do the actual loop, loading 512 bytes per iteration and storing 256.
        for (; index <= (length * 4) - 512; index += 512)
        {
            Vector512<byte> i0 = Vector512.LoadUnsafe(ref Unsafe.Add(ref s24le, index));
            Vector512<byte> i1 = Vector512.LoadUnsafe(ref Unsafe.Add(ref s24le, index + 64));
            Vector512<byte> i2 = Vector512.LoadUnsafe(ref Unsafe.Add(ref s24le, index + 128));
            Vector512<byte> i3 = Vector512.LoadUnsafe(ref Unsafe.Add(ref s24le, index + 192));

            i0 = Vector512.Add(i0.AsInt32(), rounding).AsByte();
            i1 = Vector512.Add(i1.AsInt32(), rounding).AsByte();
            i2 = Vector512.Add(i2.AsInt32(), rounding).AsByte();
            i3 = Vector512.Add(i3.AsInt32(), rounding).AsByte();

            Vector512<byte> i4 = Vector512.LoadUnsafe(ref Unsafe.Add(ref s24le, index + 256));
            Vector512<byte> i5 = Vector512.LoadUnsafe(ref Unsafe.Add(ref s24le, index + 320));
            Vector512<byte> i6 = Vector512.LoadUnsafe(ref Unsafe.Add(ref s24le, index + 384));
            Vector512<byte> i7 = Vector512.LoadUnsafe(ref Unsafe.Add(ref s24le, index + 448));

            i4 = Vector512.Add(i4.AsInt32(), rounding).AsByte();
            i5 = Vector512.Add(i5.AsInt32(), rounding).AsByte();
            i6 = Vector512.Add(i6.AsInt32(), rounding).AsByte();
            i7 = Vector512.Add(i7.AsInt32(), rounding).AsByte();

            Vector512<byte> v0 = Avx512Vbmi.PermuteVar64x8x2(i0, mask, i1);
            Vector512<byte> v1 = Avx512Vbmi.PermuteVar64x8x2(i2, mask, i3);
            Vector512<byte> v2 = Avx512Vbmi.PermuteVar64x8x2(i4, mask, i5);
            Vector512<byte> v3 = Avx512Vbmi.PermuteVar64x8x2(i6, mask, i7);

            v0.StoreAlignedNonTemporal(pTarget);
            v1.StoreAlignedNonTemporal(pTarget + 64);
            v2.StoreAlignedNonTemporal(pTarget + 128);
            v3.StoreAlignedNonTemporal(pTarget + 192);

            pTarget += 256;
        }

        // lastly, clean up the remaining bytes
        V128(ref s24le, ref s16le, index, length);
    }

    public static void Avx512VbmiLessThan256KB(ref byte s24le, ref byte s16le, int index, int length)
    {
        for (; index <= (length * 4) - 512; index += 512)
        {
            Vector512<int> rounding = Vector512.Create(0b00000000_00000000_01111111_11111111);

            Vector512<byte> i0 = Vector512.LoadUnsafe(ref Unsafe.Add(ref s24le, index));
            Vector512<byte> i1 = Vector512.LoadUnsafe(ref Unsafe.Add(ref s24le, index + 64));
            Vector512<byte> i2 = Vector512.LoadUnsafe(ref Unsafe.Add(ref s24le, index + 128));
            Vector512<byte> i3 = Vector512.LoadUnsafe(ref Unsafe.Add(ref s24le, index + 192));

            i0 = Vector512.Add(i0.AsInt32(), rounding).AsByte();
            i1 = Vector512.Add(i1.AsInt32(), rounding).AsByte();
            i2 = Vector512.Add(i2.AsInt32(), rounding).AsByte();
            i3 = Vector512.Add(i3.AsInt32(), rounding).AsByte();

            Vector512<byte> i4 = Vector512.LoadUnsafe(ref Unsafe.Add(ref s24le, index + 256));
            Vector512<byte> i5 = Vector512.LoadUnsafe(ref Unsafe.Add(ref s24le, index + 320));
            Vector512<byte> i6 = Vector512.LoadUnsafe(ref Unsafe.Add(ref s24le, index + 384));
            Vector512<byte> i7 = Vector512.LoadUnsafe(ref Unsafe.Add(ref s24le, index + 448));

            i4 = Vector512.Add(i4.AsInt32(), rounding).AsByte();
            i5 = Vector512.Add(i5.AsInt32(), rounding).AsByte();
            i6 = Vector512.Add(i6.AsInt32(), rounding).AsByte();
            i7 = Vector512.Add(i7.AsInt32(), rounding).AsByte();

            Vector512<byte> mask = Vector512.Create(Avx512VbmiMask);

            Vector512<byte> v0 = Avx512Vbmi.PermuteVar64x8x2(i0, mask, i1);
            Vector512<byte> v1 = Avx512Vbmi.PermuteVar64x8x2(i2, mask, i3);
            Vector512<byte> v2 = Avx512Vbmi.PermuteVar64x8x2(i4, mask, i5);
            Vector512<byte> v3 = Avx512Vbmi.PermuteVar64x8x2(i6, mask, i7);

            v0.StoreUnsafe(ref Unsafe.Add(ref s16le, index / 2));
            v1.StoreUnsafe(ref Unsafe.Add(ref s16le, (index / 2) + 64));
            v2.StoreUnsafe(ref Unsafe.Add(ref s16le, (index / 2) + 128));
            v3.StoreUnsafe(ref Unsafe.Add(ref s16le, (index / 2) + 192));
        }

        V128(ref s24le, ref s16le, index, length);
    }

    public static void Avx2(ref byte s24le, ref byte s16le, int index, int length)
    {
        for (; index <= (length * 4) - 128; index += 128)
        {
            Vector256<int> rounding = Vector256.Create(0b00000000_00000000_01111111_11111111);

            Vector256<byte> i0 = Vector256.LoadUnsafe(ref Unsafe.Add(ref s24le, index));
            Vector256<byte> i1 = Vector256.LoadUnsafe(ref Unsafe.Add(ref s24le, index + 32));
            Vector256<byte> i2 = Vector256.LoadUnsafe(ref Unsafe.Add(ref s24le, index + 64));
            Vector256<byte> i3 = Vector256.LoadUnsafe(ref Unsafe.Add(ref s24le, index + 96));

            i0 = Vector256.Add(i0.AsInt32(), rounding).AsByte();
            i1 = Vector256.Add(i1.AsInt32(), rounding).AsByte();
            i2 = Vector256.Add(i2.AsInt32(), rounding).AsByte();
            i3 = Vector256.Add(i3.AsInt32(), rounding).AsByte();

            Vector256<byte> mask = Vector256.Create(Avx2Mask);

            i0 = System.Runtime.Intrinsics.X86.Avx2.Shuffle(i0, mask);
            i1 = System.Runtime.Intrinsics.X86.Avx2.Shuffle(i1, mask);
            i2 = System.Runtime.Intrinsics.X86.Avx2.Shuffle(i2, mask);
            i3 = System.Runtime.Intrinsics.X86.Avx2.Shuffle(i3, mask);

            Vector128<ulong> v0 = System.Runtime.Intrinsics.X86.Avx2.Permute4x64(i0.AsUInt64(), Avx2PermuteMask).GetLower();
            Vector128<ulong> v1 = System.Runtime.Intrinsics.X86.Avx2.Permute4x64(i1.AsUInt64(), Avx2PermuteMask).GetLower();
            Vector128<ulong> v2 = System.Runtime.Intrinsics.X86.Avx2.Permute4x64(i2.AsUInt64(), Avx2PermuteMask).GetLower();
            Vector128<ulong> v3 = System.Runtime.Intrinsics.X86.Avx2.Permute4x64(i3.AsUInt64(), Avx2PermuteMask).GetLower();

            Vector256.Create(v0, v1).AsByte().StoreUnsafe(ref Unsafe.Add(ref s16le, index / 2));
            Vector256.Create(v2, v3).AsByte().StoreUnsafe(ref Unsafe.Add(ref s16le, (index / 2) + 32));
        }

        V128(ref s24le, ref s16le, index, length);
    }

    public static void V128(ref byte s24le, ref byte s16le, int index, int length)
    {
        for (; index <= (length * 4) - 64; index += 64)
        {
            Vector128<int> rounding = Vector128.Create(0b00000000_00000000_01111111_11111111);

            Vector128<byte> i0 = Vector128.LoadUnsafe(ref Unsafe.Add(ref s24le, index));
            Vector128<byte> i1 = Vector128.LoadUnsafe(ref Unsafe.Add(ref s24le, index + 16));
            Vector128<byte> i2 = Vector128.LoadUnsafe(ref Unsafe.Add(ref s24le, index + 32));
            Vector128<byte> i3 = Vector128.LoadUnsafe(ref Unsafe.Add(ref s24le, index + 48));

            i0 = Vector128.Add(i0.AsInt32(), rounding).AsByte();
            i1 = Vector128.Add(i1.AsInt32(), rounding).AsByte();
            i2 = Vector128.Add(i2.AsInt32(), rounding).AsByte();
            i3 = Vector128.Add(i3.AsInt32(), rounding).AsByte();

            // since we can't pass in two lanes like in the case of PermuteVar64x8x2, just throw the upper lane away
            Vector128<byte> mask = Vector128.Create(V128Mask);

            i0 = MemoryHelpers.Shuffle(i0, mask);
            i1 = MemoryHelpers.Shuffle(i1, mask);
            i2 = MemoryHelpers.Shuffle(i2, mask);
            i3 = MemoryHelpers.Shuffle(i3, mask);

            Unsafe.WriteUnaligned(ref Unsafe.Add(ref s16le, index / 2), i0.As<byte, ulong>().GetElement(0));
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref s16le, (index / 2) + 8), i1.As<byte, ulong>().GetElement(0));
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref s16le, (index / 2) + 16), i2.As<byte, ulong>().GetElement(0));
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref s16le, (index / 2) + 24), i3.As<byte, ulong>().GetElement(0));
        }

        // fallback loop for any elements not processed by the SIMD loop(s) above
        for (; index < (length * 4); index += 4)
        {
            int s24Value = Unsafe.ReadUnaligned<int>(ref Unsafe.Add(ref s24le, index));

            // round the value we have
            s24Value += 0b00000000_00000000_01111111_11111111;
            short value = unchecked((short)(s24Value >> 16));

            Unsafe.WriteUnaligned(ref Unsafe.Add(ref s16le, index / 2), value);
        }
    }
}
